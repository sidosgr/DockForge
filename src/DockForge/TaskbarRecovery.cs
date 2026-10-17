using System.Diagnostics;
using System.Text.Json;
namespace DockForge;
public sealed class TaskbarRecovery : IDisposable {
 readonly Dictionary<nint,string> hidden=[];
 readonly string journal=Path.Combine(Log.Root,"taskbar-recovery.json");
 Process? guard;
 public void Apply(IEnumerable<string> monitorIds){
  var desired=monitorIds.ToHashSet();
  foreach(var h in hidden.Keys.ToList())if(!desired.Contains(hidden[h])){Native.ShowWindow(h,5);hidden.Remove(h);}
  if(desired.Count>0)EnsureGuard();
  if(desired.Count>0&&guard==null)throw new InvalidOperationException("The recovery helper could not start. Windows taskbars have been left visible.");
  Native.EnumWindows((h,_)=>{if(Native.Class(h) is "Shell_TrayWnd" or "Shell_SecondaryTrayWnd"){
   var monitor=Monitor.ForWindow(h);if(desired.Contains(monitor)){
    if(!hidden.ContainsKey(h)&&Native.IsWindowVisible(h)){hidden[h]=monitor;WriteJournal();}
    if(hidden.ContainsKey(h))Native.ShowWindow(h,0);
   }
  }return true;},0);WriteJournal();
 }
 void WriteJournal(){Directory.CreateDirectory(Log.Root);File.WriteAllText(journal+".tmp",JsonSerializer.Serialize(hidden.Select(x=>new RecoveryEntry(x.Key.ToInt64(),x.Value)).ToArray()));File.Move(journal+".tmp",journal,true);}
 void EnsureGuard(){if(guard is {HasExited:false})return;try{string readyName="Local\\DockForge.GuardReady."+Guid.NewGuid();using var ready=new EventWaitHandle(false,EventResetMode.ManualReset,readyName);guard=Process.Start(new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true,ArgumentList={"--guard",Environment.ProcessId.ToString(),Process.GetCurrentProcess().StartTime.ToUniversalTime().Ticks.ToString(),journal,readyName}});if(!ready.WaitOne(8000)||guard==null||guard.HasExited){Log.Write("Recovery helper did not become ready.");guard=null;}}catch(Exception e){Log.Write(e);guard=null;}}
 public void Restore(){foreach(var h in hidden.Keys)if(Native.IsWindow(h))Native.ShowWindow(h,5);hidden.Clear();WriteJournal();}
 public void Dispose(){Restore();}
 public static void Recover(string journal){try{if(!File.Exists(journal))return;var entries=JsonSerializer.Deserialize<RecoveryEntry[]>(File.ReadAllText(journal))??[];foreach(var e in entries){nint h=(nint)e.Handle;if(Native.IsWindow(h)&&Native.Class(h) is "Shell_TrayWnd" or "Shell_SecondaryTrayWnd")Native.ShowWindow(h,5);}File.WriteAllText(journal,"[]");}catch(Exception e){Log.Write(e);}}
 public static void Guard(string[] args){try{using var p=Process.GetProcessById(int.Parse(args[1]));if(p.StartTime.ToUniversalTime().Ticks==long.Parse(args[2])){if(args.Length>4){using var ready=EventWaitHandle.OpenExisting(args[4]);ready.Set();}p.WaitForExit();}}catch{}Recover(args[3]);}
 public record RecoveryEntry(long Handle,string Monitor);
}
