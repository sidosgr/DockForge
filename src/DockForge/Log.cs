namespace DockForge;
public static class Log {
 public static string Root {get;set;}=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DockForge");
 public static void Write(object message){try{Directory.CreateDirectory(Root);string p=Path.Combine(Root,"diagnostics.log");if(File.Exists(p)&&new System.IO.FileInfo(p).Length>1_000_000)File.Move(p,p+".1",true);File.AppendAllText(p,$"{DateTimeOffset.Now:O} {message}\n");}catch{}}
}
