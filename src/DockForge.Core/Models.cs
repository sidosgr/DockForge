using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockForge.Core;
public enum BarMode { Launcher, RunningApps, Combined }
public enum Edge { Left, Top, Right, Bottom, Floating }
public enum VisibilityMode { ReserveSpace, Overlay, AutoHide }
public enum LengthMode { Full, FitContents, Custom }
public enum Alignment { Start, Center, End }
public enum BarStyle { System, Windows10, Windows11, WindowsXP }
public enum ThemeMode { System, Light, Dark }
public enum LabelMode { IconsOnly, AllItems, RunningWindows }
public enum ItemKind { Shortcut, Website, Group, LinkedFolder, Separator, Spacer }
public enum GroupLayout { Grid, List }
public enum GroupSort { Manual, Name, Type, Date }
public enum FolderFilter { Everything, Shortcuts, Extensions }
public enum FloatingReveal { Tab, Shortcut, Both }
public enum MiddleAction { Close, NewInstance, None }
public enum AppFilter { All, Include, Exclude }

public sealed class Settings {
 public int SchemaVersion { get; set; } = 1;
 public bool StartAtSignIn { get; set; } = true;
 public bool ConfirmCloseAll { get; set; } = true;
 public bool CheckUpdates { get; set; } = true;
 public bool IncludePreviewUpdates { get; set; } = true;
 public List<string> HiddenTaskbarMonitors { get; set; } = [];
 public List<Bar> Bars { get; set; } = [];
 public bool SetupComplete { get; set; }
 public Settings Clone() => JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(this, Json), Json)!;
 public static readonly JsonSerializerOptions Json = new() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
}
public sealed class Bar {
 public override string ToString()=>Name;
 public Guid Id { get; set; } = Guid.NewGuid();
 public string Name { get; set; } = "My bar";
 public List<string> Monitors { get; set; } = [];
 public Edge Edge { get; set; } = Edge.Top;
 public BarMode Mode { get; set; } = BarMode.Combined;
 public bool Vertical { get; set; }
 public VisibilityMode Visibility { get; set; } = VisibilityMode.Overlay;
 public LengthMode LengthMode { get; set; } = LengthMode.FitContents;
 public double Length { get; set; } = 640;
 public Alignment Alignment { get; set; } = Alignment.Center;
 public double FloatX { get; set; } = 100;
 public double FloatY { get; set; } = 100;
 public bool Locked { get; set; }
 public BarStyle Style { get; set; }
 public ThemeMode Theme { get; set; }
 public double IconSize { get; set; } = 28;
 public double Thickness { get; set; } = 52;
 public double Spacing { get; set; } = 4;
 public double Opacity { get; set; } = .97;
 public bool Blur { get; set; } = true;
 public string Background { get; set; } = "";
 public string Accent { get; set; } = "#60CDFF";
 public string HoverColor { get; set; } = "#26808080";
 public double CornerRadius { get; set; } = -1;
 public LabelMode Labels { get; set; }
 public bool Indicators { get; set; } = true;
 public bool Badges { get; set; } = true;
 public bool CurrentMonitorOnly { get; set; } = true;
 public bool CombineWindows { get; set; } = true;
 public bool Thumbnails { get; set; } = true;
 public bool SwitchOnClick { get; set; } = true;
 public bool MinimizeActive { get; set; } = true;
 public MiddleAction MiddleClick { get; set; } = MiddleAction.Close;
 public AppFilter AppFilter { get; set; }
 public string AppFilterList { get; set; } = "";
 public List<string> RunningOrder { get; set; } = [];
 public int RevealDelay { get; set; } = 250;
 public int HideDelay { get; set; } = 700;
 public bool ActivationStrip { get; set; } = true;
 public FloatingReveal FloatingReveal { get; set; } = FloatingReveal.Both;
 public string Hotkey { get; set; } = "";
 public List<Item> Items { get; set; } = [];
 [JsonIgnore] public bool IsVertical => Edge is Edge.Left or Edge.Right || (Edge == Edge.Floating && Vertical);
}
public sealed class Item {
 public override string ToString()=>Name;
 public Guid Id { get; set; } = Guid.NewGuid();
 public ItemKind Kind { get; set; }
 public string Name { get; set; } = "";
 public string Target { get; set; } = "";
 public string AppId { get; set; } = "";
 public string Arguments { get; set; } = "";
 public string WorkingDirectory { get; set; } = "";
 public string Icon { get; set; } = "";
 public double SpacerSize { get; set; } = 24;
 public List<Item> Children { get; set; } = [];
 public GroupLayout Layout { get; set; }
 public GroupSort Sort { get; set; } = GroupSort.Name;
 public bool Descending { get; set; }
 public FolderFilter Filter { get; set; }
 public string Extensions { get; set; } = ".lnk;.url;.exe";
 public List<string> ManualOrder { get; set; } = [];
 public double PopupWidth { get; set; } = 360;
 public double PopupHeight { get; set; } = 380;
 public bool GroupShowLabels { get; set; } = true;
 public bool HideShortcutBadges { get; set; } = true;
 public double GroupIconSize { get; set; } = 32;
 public bool OpenOnHover { get; set; }
 public int HoverDelay { get; set; } = 450;
 public bool KeepOpen { get; set; }
 [JsonIgnore] public bool IsGroup => Kind is ItemKind.Group or ItemKind.LinkedFolder;
 public static Item FromPath(string path) => new() { Name = Path.GetFileNameWithoutExtension(path), Target = path };
}
public readonly record struct Bounds(double X, double Y, double Width, double Height);
public static class Layout {
 public static double MinimumThickness(double iconSize,double spacing)=>Math.Clamp(Math.Ceiling(iconSize+22+spacing),32,200);
 public static int InsertionIndex(double coordinate,IReadOnlyList<(double Start,double Length)> items){for(int i=0;i<items.Count;i++)if(coordinate<items[i].Start+items[i].Length/2)return i;return items.Count;}
 public static (double X,double Y) RevealOffset(Edge edge,double width,double height,double progress){double remaining=1-Math.Clamp(progress,0,1);return edge switch{Edge.Left=>(-width*remaining,0),Edge.Right=>(width*remaining,0),Edge.Top=>(0,-height*remaining),Edge.Bottom=>(0,height*remaining),_=>(0,12*remaining)};}
 public static Bounds Place(Bar b, Bounds screen, double contentLength) {
  bool v = b.IsVertical;
  double available = v ? screen.Height : screen.Width;
  double length = Math.Clamp(b.LengthMode switch { LengthMode.Full => available, LengthMode.Custom => b.Length, _ => contentLength }, 48, Math.Max(48, available));
  double thick = Math.Clamp(b.Thickness, 32, 200);
  double offset = b.Alignment switch { Alignment.Start => 0, Alignment.End => available - length, _ => (available - length) / 2 };
  if (b.Edge == Edge.Floating) return new(screen.X + Math.Clamp(b.FloatX, 0, Math.Max(0, screen.Width-(v?thick:length))), screen.Y + Math.Clamp(b.FloatY, 0, Math.Max(0, screen.Height-(v?length:thick))), v?thick:length, v?length:thick);
  return b.Edge switch {
   Edge.Top => new(screen.X+offset,screen.Y,length,thick), Edge.Bottom => new(screen.X+offset,screen.Y+screen.Height-thick,length,thick),
   Edge.Left => new(screen.X,screen.Y+offset,thick,length), _ => new(screen.X+screen.Width-thick,screen.Y+offset,thick,length)
  };
 }
 public static List<string> Validate(Settings settings) {
  var errors = new List<string>(); var used = new HashSet<string>();
  if (settings.SchemaVersion != 1) errors.Add("This settings version is not supported.");
  if(settings.Bars==null||settings.HiddenTaskbarMonitors==null){errors.Add("Settings collections cannot be null.");return errors;}
  if(settings.Bars.Count>100){errors.Add("A setup can contain at most 100 bars.");return errors;}
  var ids=new HashSet<Guid>();
  foreach(var b in settings.Bars) {
   if(b==null||b.Monitors==null||b.Items==null||b.RunningOrder==null){errors.Add("Bar data is incomplete.");continue;}
   if(!ids.Add(b.Id))errors.Add("Bar identifiers must be unique.");
   if(!Enum.IsDefined(b.Edge)||!Enum.IsDefined(b.Mode)||!Enum.IsDefined(b.Visibility)||!Enum.IsDefined(b.LengthMode)||!Enum.IsDefined(b.Theme)||!Enum.IsDefined(b.Style)||!Enum.IsDefined(b.Alignment))errors.Add($"{b.Name}: invalid option value.");
   if(new[]{b.Length,b.FloatX,b.FloatY,b.IconSize,b.Thickness,b.Spacing,b.Opacity,b.CornerRadius}.Any(n=>!double.IsFinite(n)))errors.Add($"{b.Name}: dimensions must be finite.");
   if(b.IconSize is <16 or >96||b.Thickness is <32 or >200||b.Spacing is <0 or >40||b.Opacity is <.2 or >1||b.Length is <48 or >16000)errors.Add($"{b.Name}: appearance value is outside the supported range.");
   if(b.RevealDelay is <0 or >5000||b.HideDelay is <100 or >10000)errors.Add($"{b.Name}: invalid auto-hide timing.");
   if (b.Monitors.Count == 0) errors.Add($"{b.Name}: select at least one monitor.");
   foreach(var m in b.Monitors.Distinct()) if(b.Edge!=Edge.Floating && !used.Add($"{m}|{b.Edge}")) errors.Add($"{b.Name}: {b.Edge} is already occupied on {m}.");
   if(b.Thickness < b.IconSize+12) errors.Add($"{b.Name}: thickness must be at least icon size + 12.");
   foreach(var i in b.Items){if(i==null||i.Children==null||i.ManualOrder==null){errors.Add($"{b.Name}: item data is incomplete.");continue;}if(!Enum.IsDefined(i.Kind)||!Enum.IsDefined(i.Sort)||!Enum.IsDefined(i.Layout)||!Enum.IsDefined(i.Filter))errors.Add($"{b.Name}: invalid item option.");if(i.Children.Any(c=>c==null||c.IsGroup))errors.Add($"{b.Name}: nested groups are not supported.");if(i.PopupWidth is <200 or >1000||i.PopupHeight is <150 or >1000||i.GroupIconSize is <16 or >96)errors.Add($"{b.Name}: popup dimensions are outside the supported range.");}
   if (b.Edge == Edge.Floating && b.Visibility == VisibilityMode.ReserveSpace) errors.Add($"{b.Name}: floating bars cannot reserve screen space.");
  }
  return errors;
 }
}
