using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace DockForge;
public sealed record Monitor(string Id,string Name,Bounds Bounds,Bounds Work,double Scale,bool Primary,string DeviceName) {
 static DateTime last;static List<Monitor> cached=[];
 public static List<Monitor> All(){if(DateTime.UtcNow-last<TimeSpan.FromSeconds(1)&&cached.Count>0)return cached;last=DateTime.UtcNow;cached=Screen.AllScreens.Select((s,i)=>{
  var h=Native.MonitorFromPoint(new Native.Pt{X=s.Bounds.X+1,Y=s.Bounds.Y+1},2);
  Native.GetDpiForMonitor(h,0,out var dpi,out _);double scale=Math.Max(1,dpi/96d);
  var device=new Native.DisplayDevice{Size=Marshal.SizeOf<Native.DisplayDevice>()};Native.EnumDisplayDevices(s.DeviceName,0,ref device,1);string id=string.IsNullOrWhiteSpace(device.Id)?s.DeviceName:device.Id;
  return new Monitor(id,$"Display {i+1}{(s.Primary?" · Primary":"")} · {s.Bounds.Width} × {s.Bounds.Height}",new(s.Bounds.X,s.Bounds.Y,s.Bounds.Width,s.Bounds.Height),new(s.WorkingArea.X,s.WorkingArea.Y,s.WorkingArea.Width,s.WorkingArea.Height),scale,s.Primary,s.DeviceName);
 }).ToList();return cached;}
 public static string ForWindow(nint h){string name=Native.MonitorName(h);return All().FirstOrDefault(m=>m.DeviceName==name)?.Id??name;}
 public override string ToString()=>Name;
}
public sealed record AppWindow(nint Handle,string Title,string Path,string AppId,string Monitor,bool Active) {
 public string Key=>!string.IsNullOrEmpty(AppId)?AppId:Path.ToLowerInvariant();
 public Item AsItem()=>new(){Name=System.IO.Path.GetFileNameWithoutExtension(Path),Target=Path,AppId=AppId};
}
[ComImport,Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IVirtualDesktopManager {
 [PreserveSig] int IsWindowOnCurrentVirtualDesktop(nint topLevelWindow,[MarshalAs(UnmanagedType.Bool)]out bool onCurrentDesktop);
 [PreserveSig] int GetWindowDesktopId(nint topLevelWindow,out Guid desktopId);
 [PreserveSig] int MoveWindowToDesktop(nint topLevelWindow,ref Guid desktopId);
}
public sealed class WindowsService {
 readonly IVirtualDesktopManager? desktop;
 public HashSet<nint> Attention {get;}=[];
 public WindowsService(){try{desktop=(IVirtualDesktopManager)Activator.CreateInstance(Type.GetTypeFromCLSID(new("AA509086-5CA9-4C25-8F95-589D3C07B48A"))!)!;}catch(Exception e){Log.Write(e);}}
 public List<AppWindow> Enumerate(){
  var windows=new List<AppWindow>();var active=Native.GetForegroundWindow();
  Native.EnumWindows((h,_)=>{
   if(!Native.IsWindowVisible(h)||Native.GetWindow(h,4)!=0)return true;
   Native.GetWindowThreadProcessId(h,out var pid);if(pid==Environment.ProcessId)return true;
   if(((long)Native.GetWindowLongPtr(h,-20)&0x80)!=0)return true;
   var title=Native.Title(h);if(string.IsNullOrWhiteSpace(title))return true;
   var cls=Native.Class(h);if(cls is "Shell_TrayWnd" or "Shell_SecondaryTrayWnd" or "Progman" or "WorkerW")return true;
   Native.DwmGetWindowAttribute(h,14,out int cloaked,4);if(cloaked!=0)return true;
   try{if(desktop!=null&&desktop.IsWindowOnCurrentVirtualDesktop(h,out var current)==0&&!current)return true;}catch{}
   string path=Native.ProcessPath(h,out var appId);string windowAppId=ShellIdentity.AppId(h);if(windowAppId.Length>0)appId=windowAppId;
   windows.Add(new(h,title,path,appId,Monitor.ForWindow(h),h==active));
   return true;
  },0);
  Attention.RemoveWhere(h=>!Native.IsWindow(h)||h==active);
  return windows;
 }
 public static void Activate(nint h,bool minimize=false){if(!Native.IsWindow(h))return;if(minimize&&Native.GetForegroundWindow()==h){Native.ShowWindowAsync(h,6);return;}if(Native.IsIconic(h))Native.ShowWindowAsync(h,9);Native.SetForegroundWindow(h);}
 public static void Close(nint h){if(Native.IsWindow(h))Native.PostMessage(h,0x10,0,0);}
 public static bool Fullscreen(string monitor){var h=Native.GetForegroundWindow();if(h==0||Native.IsIconic(h)||Monitor.ForWindow(h)!=monitor)return false;Native.GetWindowThreadProcessId(h,out var pid);if(pid==Environment.ProcessId)return false;var cls=Native.Class(h);if(cls is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd")return false;var m=Monitor.All().Find(m=>m.Id==monitor);if(m==null)return false;Native.GetWindowRect(h,out var r);return r.Left<=m.Bounds.X&&r.Top<=m.Bounds.Y&&r.Right>=m.Bounds.X+m.Bounds.Width&&r.Bottom>=m.Bounds.Y+m.Bounds.Height;}
}
