using System.Text.Json;
namespace DockForge.Core;
public sealed class SettingsStore(string root) {
 public string Root { get; } = root;
 public string FilePath => Path.Combine(Root,"settings.json");
 public Settings Load() {
  Directory.CreateDirectory(Root);
  if(!File.Exists(FilePath)) return new();
  return Read(FilePath);
 }
 public static Settings Read(string path) {
  var info = new FileInfo(path); if(info.Length>10_000_000) throw new InvalidDataException("Settings file is too large.");
  var s = JsonSerializer.Deserialize<Settings>(File.ReadAllText(path),Settings.Json) ?? throw new InvalidDataException("Empty settings file.");
  var errors=Layout.Validate(s); if(errors.Count>0) throw new InvalidDataException(string.Join("\n",errors));
  return s;
 }
 public void Save(Settings settings) {
  Directory.CreateDirectory(Root);
  string temp=FilePath+".tmp";
  File.WriteAllText(temp,JsonSerializer.Serialize(settings,Settings.Json));
  File.Move(temp,FilePath,true);
 }
 public void Backup() {
  if(!File.Exists(FilePath)) return;
  var dir=Path.Combine(Root,"Backups"); Directory.CreateDirectory(dir);
  File.Copy(FilePath,Path.Combine(dir,$"settings-{DateTime.UtcNow:yyyyMMdd-HHmmss-ffff}.json"));
  foreach(var old in new DirectoryInfo(dir).GetFiles("*.json").OrderByDescending(f=>f.Name).Skip(8)) old.Delete();
 }
 public void Import(string path) { var s=Read(path); Backup(); Save(s); }
}
