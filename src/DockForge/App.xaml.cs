using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
namespace DockForge;
public partial class App : Application {
 public static App Host=>(App)Current;
 public Settings Config {get;set;}=new();
 public SettingsStore Store {get;private set;}=null!;
 public new WindowsService Windows {get;}=new();
 public List<AppWindow> Running {get;private set;}=[];
 public List<BarWindow> Bars {get;}=[];
 public TaskbarRecovery Recovery {get;private set;}=null!;
 public bool Sandbox {get;private set;}
 public bool SettingsOpen=>Application.Current.Windows.OfType<SettingsWindow>().Any(w=>w.IsVisible);
 public bool UiTest {get;private set;}
 EventWaitHandle? settingsSignal;RegisteredWaitHandle? settingsWait;
 SettingsWindow? settingsWindow;System.Windows.Forms.NotifyIcon? tray;Mutex? singleton;DispatcherTimer? pulse;HwndSource? messages;
 uint shellMessage,taskbarCreated; string monitorSignature="";int ticks;bool stopping;
 protected override void OnStartup(StartupEventArgs e){base.OnStartup(e);
  if(e.Args.FirstOrDefault()=="--guard-fixture"){Thread.Sleep(7000);Shutdown();return;} if(e.Args.FirstOrDefault()=="--guard"){TaskbarRecovery.Guard(e.Args);Shutdown();return;}
  Sandbox=e.Args.Contains("--sandbox");UiTest=Sandbox&&e.Args.Contains("--ui-test");if(Sandbox)Log.Root=Path.Combine(AppContext.BaseDirectory,"sandbox-profile");
  if(e.Args.Contains("--restore-taskbars")){TaskbarRecovery.Recover(Path.Combine(Log.Root,"taskbar-recovery.json"));Shutdown();return;}
  singleton=new Mutex(true,"Local\\DockForge"+(Sandbox?".Sandbox":""),out bool first);if(!first){try{using var signal=EventWaitHandle.OpenExisting("Local\\DockForge.Settings"+(Sandbox?".Sandbox":""));signal.Set();}catch(WaitHandleCannotBeOpenedException){MessageBox.Show("DockForge is starting. Try again in a moment.","DockForge");}Shutdown();return;}
  settingsSignal=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\DockForge.Settings"+(Sandbox?".Sandbox":""));Store=new(Sandbox&&e.Args.Contains("--smoke-test")?Path.Combine(Log.Root,"smoke-profile"):Log.Root);Recovery=new();TaskbarRecovery.Recover(Path.Combine(Log.Root,"taskbar-recovery.json"));
  DispatcherUnhandledException+=(s,a)=>{Log.Write(a.Exception);Recovery.Restore();MessageBox.Show("DockForge encountered an error and restored Windows taskbars.\n"+a.Exception.Message,"DockForge");a.Handled=true;};
  try{Config=Store.Load();}catch(Exception ex){Log.Write(ex);MessageBox.Show("Settings could not be loaded. The original file is preserved. You can restore a backup in Settings.\n"+ex.Message,"DockForge recovery");Config=new();}
  if(e.Args.Contains("--demo")&&Config.Bars.Count==0){Config.SetupComplete=true;Config.StartAtSignIn=false;Config.Bars.Add(new(){Name="My workspace",Monitors=[Monitor.All().First().Id],Items=[new(){Name="File Explorer",Target="C:\\Windows\\explorer.exe"},new(){Name="Windows",Kind=ItemKind.LinkedFolder,Target="C:\\Windows",Filter=FolderFilter.Extensions,Extensions=".exe"},new(){Name="Web",Kind=ItemKind.Website,Target="https://example.com"}]});}
  messages=new HwndSource(new HwndSourceParameters("DockForge messages"){Width=0,Height=0,WindowStyle=0});messages.AddHook(Hook);shellMessage=Native.RegisterWindowMessage("SHELLHOOK");taskbarCreated=Native.RegisterWindowMessage("TaskbarCreated");Native.RegisterShellHookWindow(messages.Handle);
  tray=new(){Text="DockForge",Icon=new System.Drawing.Icon(Path.Combine(AppContext.BaseDirectory,"Assets","dockforge.ico")),Visible=true};var menu=new System.Windows.Forms.ContextMenuStrip();menu.Items.Add("Settings",null,(_,_)=>ShowSettings());menu.Items.Add("Restore Windows taskbars",null,(_,_)=>RestoreTaskbars());menu.Items.Add("Exit DockForge",null,(_,_)=>Quit());tray.ContextMenuStrip=menu;tray.DoubleClick+=(_,_)=>ShowSettings();
  Running=Windows.Enumerate();RebuildBars();pulse=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(250)};pulse.Tick+=(_,_)=>Tick();pulse.Start();SessionEnding+=(_,_)=>Quit();
  settingsWait=ThreadPool.RegisterWaitForSingleObject(settingsSignal,(_,_)=>Dispatcher.BeginInvoke(()=>ShowSettings()),null,Timeout.Infinite,false);SystemEvents.UserPreferenceChanged+=ThemeChanged; if(e.Args.Contains("--smoke-test")){Dispatcher.BeginInvoke(async()=>await SmokeTests.Run());return;} if(!Config.SetupComplete)new SetupWizard().Show();else if(e.Args.Contains("--settings"))ShowSettings();
  if(!Sandbox&&Config.SetupComplete)SetStartup();
  if(!Sandbox&&Config.CheckUpdates)Dispatcher.BeginInvoke(async()=>await UpdateService.Client.CheckAsync(Config.IncludePreviewUpdates));
 }
 nint Hook(nint h,int msg,nint w,nint l,ref bool handled){if((uint)msg==shellMessage&&(long)w==0x8006)Windows.Attention.Add(l);if((uint)msg==taskbarCreated)Dispatcher.BeginInvoke(RebuildBars);if(msg==0x312){var source=Bars.FirstOrDefault(b=>b.HotkeyId==(int)w);if(source!=null)foreach(var b in Bars.Where(b=>b.Model.Id==source.Model.Id))b.ToggleReveal();handled=true;}return 0;}
 void Tick(){if(stopping)return;ticks++;foreach(var bar in Bars.ToArray())bar.Pulse();if(ticks%4!=0)return;Running=Windows.Enumerate();foreach(var bar in Bars.ToArray())bar.RefreshItems();string sig=string.Join(";",Monitor.All().Select(m=>$"{m.Id}{m.Bounds}{m.Scale}"));if(sig!=monitorSignature){monitorSignature=sig;RebuildBars();}if(!Sandbox&&Config.HiddenTaskbarMonitors.Count>0)try{Recovery.Apply(Config.HiddenTaskbarMonitors);}catch(Exception e){Log.Write(e);}}
 public void RebuildBars(){monitorSignature=string.Join(";",Monitor.All().Select(m=>$"{m.Id}{m.Bounds}{m.Scale}"));foreach(var b in Bars.ToArray())b.Close();Bars.Clear();int hotkey=100;foreach(var b in Config.Bars)foreach(var monitor in Monitor.All().Where(m=>b.Monitors.Contains(m.Id))){var w=new BarWindow(b,monitor){HotkeyId=hotkey++};Bars.Add(w);w.Show();}if(!Sandbox)Recovery.Apply(Config.HiddenTaskbarMonitors);}
 void ThemeChanged(object sender,UserPreferenceChangedEventArgs e)=>Dispatcher.BeginInvoke(RefreshBars);
 public void RefreshBars(){foreach(var b in Bars)b.ApplyAppearance();}
 public void Save(){if(settingsWindow==null)Store.Save(Config);}
 public void Commit(){var errors=Layout.Validate(Config);if(errors.Count>0)throw new InvalidDataException(string.Join("\n",errors));Store.Save(Config);if(!Sandbox)SetStartup();RebuildBars();}
 public void ShowSettings(bool wizard=false,Bar? bar=null){if(settingsWindow!=null){settingsWindow.Activate();return;}settingsWindow=new SettingsWindow(wizard,bar);settingsWindow.Closed+=(_,_)=>settingsWindow=null;settingsWindow.Show();}
 public void RestoreTaskbars(){Config.HiddenTaskbarMonitors.Clear();Recovery.Restore();Save();}
 public void SetStartup(){try{using var key=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");if(Config.StartAtSignIn)key.SetValue("DockForge","\""+Environment.ProcessPath+"\"");else key.DeleteValue("DockForge",false);}catch(Exception e){Log.Write(e);}}
 public nint MessageHandle=>messages?.Handle??0;
 public void Quit(){if(stopping)return;stopping=true;UpdateService.Cancel();pulse?.Stop();Recovery?.Restore();foreach(var b in Bars.ToArray())b.Close();tray?.Dispose();if(messages!=null){Native.DeregisterShellHookWindow(messages.Handle);messages.Dispose();}Shutdown();}
 protected override void OnExit(ExitEventArgs e){settingsWait?.Unregister(null);settingsSignal?.Dispose();SystemEvents.UserPreferenceChanged-=ThemeChanged;Recovery?.Dispose();tray?.Dispose();singleton?.Dispose();base.OnExit(e);}
}
