using DockForge.Core;
using System.Text.Json;
int assertions=0;void Check(bool condition,string name){assertions++;if(!condition)throw new Exception(name);Console.WriteLine("PASS "+name);}
var negative=new Bounds(-1920,0,1920,1080);var b=new Bar{Monitors=["A"],Edge=Edge.Left,Thickness=50,LengthMode=LengthMode.Custom,Length=400};var r=Layout.Place(b,negative,200);Check(r.X==-1920&&r.Y==340&&r.Width==50&&r.Height==400,"Vertical bar on negative-coordinate monitor");
b.Edge=Edge.Bottom;b.LengthMode=LengthMode.Full;r=Layout.Place(b,negative,200);Check(r.X==-1920&&r.Y==1030&&r.Width==1920,"Bottom bar full width");
b.Edge=Edge.Floating;b.FloatX=99999;b.FloatY=99999;b.LengthMode=LengthMode.Custom;b.Length=300;r=Layout.Place(b,negative,200);Check(r.X+r.Width<=0&&r.Y+r.Height<=1080,"Floating bar clamped to monitor");
b.Edge=Edge.Top;b.LengthMode=LengthMode.FitContents;r=Layout.Place(b,negative,99999);Check(r.Width==1920,"Overflow never expands beyond screen");
foreach(var edge in new[]{Edge.Left,Edge.Right,Edge.Top,Edge.Bottom}){var start=Layout.RevealOffset(edge,60,800,0);var middle=Layout.RevealOffset(edge,60,800,.5);var end=Layout.RevealOffset(edge,60,800,1);Check(end==(0d,0d)&&middle.X==start.X/2&&middle.Y==start.Y/2&&(edge is Edge.Left or Edge.Right?Math.Abs(start.X)==60&&start.Y==0:Math.Abs(start.Y)==800&&start.X==0),$"{edge} reveal moves from outside the viewport to its resting position");}
var xpProfile=new Settings{Bars=[new Bar{Style=BarStyle.WindowsXP}]};Check(xpProfile.Clone().Bars[0].Style==BarStyle.WindowsXP,"Windows XP style survives saved settings and snapshots");Check((int)BarStyle.Windows10==1&&(int)BarStyle.Windows11==2,"Existing Windows style identifiers remain compatible");
Check(Layout.MinimumThickness(48,4)==74&&Layout.MinimumThickness(16,4)==42,"Icon size computes a compact thickness in both directions");
Check(Layout.MinimumThickness(28,20)>Layout.MinimumThickness(28,4),"Minimum thickness leaves room for item spacing");
var slots=new List<(double Start,double Length)>{(0,40),(44,40),(88,40)};Check(Layout.InsertionIndex(10,slots)==0&&Layout.InsertionIndex(30,slots)==1&&Layout.InsertionIndex(1000,slots)==3,"Drag insertion follows icon halves and trailing space");
var groupOptions=new Item{Kind=ItemKind.Group,GroupShowLabels=false,HideShortcutBadges=true};var groupRoundTrip=JsonSerializer.Deserialize<Item>(JsonSerializer.Serialize(groupOptions,Settings.Json),Settings.Json)!;Check(!groupRoundTrip.GroupShowLabels&&groupRoundTrip.HideShortcutBadges,"Group appearance options survive settings round trip");
Check(JsonSerializer.Deserialize<Item>("{}",Settings.Json) is {GroupShowLabels:true,HideShortcutBadges:true},"Existing groups retain labels and default to clean shortcut icons");
var s=new Settings{Bars=[b]};Check(Layout.Validate(s).Count==0,"Valid setup accepted");s.Bars.Add(new(){Monitors=["A"],Edge=Edge.Top});Check(Layout.Validate(s).Any(x=>x.Contains("occupied")),"Duplicate monitor edge rejected");s.Bars[1].Edge=Edge.Floating;Check(Layout.Validate(s).Count==0,"Multiple floating bars allowed");s.Bars[1].Visibility=VisibilityMode.ReserveSpace;Check(Layout.Validate(s).Any(x=>x.Contains("reserve")),"Floating reservation rejected");s.Bars.RemoveAt(1);
b.Items.Add(new(){Kind=ItemKind.Group,Children=[new(){Kind=ItemKind.Group}]});Check(Layout.Validate(s).Any(x=>x.Contains("nested")),"Nested groups rejected");b.Items.Clear();b.MiddleClick=MiddleAction.Close;b.Monitors.Add("B");b.Hotkey="Ctrl+Alt+D";
var clone=s.Clone();clone.Bars[0].Name="Changed";Check(s.Bars[0].Name!="Changed","Cancel snapshot deep-clones state");Check(clone.Bars[0].Monitors.SequenceEqual(b.Monitors)&&clone.Bars[0].MiddleClick==MiddleAction.Close,"Mirroring and behavior survive round trip");
string root=Path.Combine(Path.GetTempPath(),"DockForge-tests-"+Guid.NewGuid());try{var store=new SettingsStore(root);store.Save(s);Check(store.Load().Bars.Count==1,"Atomic save and load");for(int i=0;i<12;i++)store.Backup();Check(Directory.GetFiles(Path.Combine(root,"Backups")).Length==8,"Backup retention keeps eight snapshots");string bad=Path.Combine(root,"bad.json");File.WriteAllText(bad,"{ invalid");try{store.Import(bad);throw new Exception("Invalid import accepted");}catch(JsonException){}Check(store.Load().Bars.Count==1,"Invalid import preserves current setup");}finally{Directory.Delete(root,true);}
object Release(string tag,bool preview=false,bool draft=false,string? file=null,string? url=null) {
 string name=file??$"DockForge-Setup-{tag.TrimStart('v')}.exe";
 return new {tag_name=tag,prerelease=preview,draft,assets=new[]{new{name,browser_download_url=url??$"https://github.com/sidosgr/DockForge/releases/download/{tag}/{name}"}}};
}
ReleaseUpdate? Find(Version current,bool previews,params object[] releases)=>UpdateFeed.FindUpdate(JsonSerializer.Serialize(releases),current,previews,"x64");
Check(UpdateFeed.Repository=="sidosgr/DockForge"&&UpdateFeed.ApiUrl.Contains("/sidosgr/DockForge/releases?"),"Update source is the official repository");
Check(Find(new(0,1,4),true,Release("v0.1.5",true)) is {Tag:"v0.1.5",IsPreview:true},"Development previews can update development builds");
Check(Find(new(0,1,4),false,Release("v0.1.5",true)) is null,"Stable-only checks exclude prereleases");
Check(Find(new(0,1,4),true,Release("v0.1.5",draft:true)) is null,"Draft releases never trigger updates");
Check(Find(new(0,1,5,0),true,Release("v0.1.5"),Release("v0.1.4")) is null,"Current assembly version and older releases do not trigger updates");
Check(Find(new(0,1,4),true,Release("v0.1.5"),Release("v0.1.7"),Release("v0.1.6")) is {Tag:"v0.1.7"},"Newest compatible version wins regardless of API order");
Check(Find(new(0,1,4),true,Release("latest"),Release("v0.1.5",file:"DockForge-Setup-0.1.5-arm64.exe")) is null,"Malformed versions and wrong architecture assets are ignored");
Check(Find(new(0,1,4),true,Release("v0.1.5",url:"https://example.com/DockForge-Setup-0.1.5.exe"),Release("v0.1.6",url:"https://github.com/other/repo/releases/download/v0.1.6/DockForge-Setup-0.1.6.exe")) is null,"Downloads outside the official repository are rejected");
Check(Find(new(0,1,4),true,Release("v0.1.5",file:"DockForge-Setup-0.1.5-x64.exe")) is not null,"Legacy architecture-specific installers remain compatible");
Check(UpdateFeed.FindUpdate(JsonSerializer.Serialize(new[]{Release("v0.1.5",file:"DockForge-Setup-0.1.5-arm64.exe")}),new(0,1,4),true,"arm64") is not null,"ARM64 selects its compatible installer");
Check(Find(new(0,1,4),true) is null,"An empty release repository is handled without failure");
var migrated=JsonSerializer.Deserialize<Settings>("{\"UpdateRepository\":\"other/repo\",\"CheckUpdates\":false}",Settings.Json)!;
Check(!migrated.CheckUpdates&&migrated.IncludePreviewUpdates&&!JsonSerializer.Serialize(migrated,Settings.Json).Contains("other/repo"),"Old repository settings cannot redirect updates and preserve the disabled preference");
var stableSettings=new Settings{IncludePreviewUpdates=false};Check(!stableSettings.Clone().IncludePreviewUpdates,"Update channel preference survives settings round trip");
if(args.Contains("--live-updates")) {
 using var client=new System.Net.Http.HttpClient{Timeout=TimeSpan.FromSeconds(20)};client.DefaultRequestHeaders.UserAgent.ParseAdd("DockForge-Update-Verification/0.1.5");
 string liveJson=await client.GetStringAsync(UpdateFeed.ApiUrl);
 Check(UpdateFeed.FindUpdate(liveJson,new(0,0,0),true,"x64") is not null,"Live official GitHub releases resolve a compatible download");
 Check(UpdateFeed.FindUpdate(liveJson,new(99,0,0),true,"arm64") is null,"Live official feed does not offer older releases as updates");
}
await UpdateScenarios.Run(Check);
if(args.Contains("--live-download")) {
 using var http=new System.Net.Http.HttpClient{Timeout=Timeout.InfiniteTimeSpan};http.DefaultRequestHeaders.UserAgent.ParseAdd("DockForge-Download-Verification/0.1.7");
 string cache=Path.Combine(Path.GetTempPath(),"DockForge-live-update-"+Guid.NewGuid().ToString("N"));
 var updates=new UpdateClient(http,cache,new Version(0,0,0),"x64");
 await updates.CheckAsync(true);Check(updates.State.Phase==UpdatePhase.Available,"Live GitHub release supplies verified download metadata");
 await updates.DownloadAsync();Check(updates.State.Phase==UpdatePhase.Ready,"Actual GitHub installer downloads and verifies inside the update client");
 string downloaded=Path.Combine(cache,Path.GetFileName(new Uri(updates.State.Release!.DownloadUrl).AbsolutePath));
 Check(await UpdateDownload.VerifyAsync(downloaded,updates.State.Release,CancellationToken.None),"Downloaded GitHub installer passes a second checksum verification without being launched");
 File.Delete(downloaded);Directory.Delete(cache);
}
Console.WriteLine($"{assertions} assertions passed.");
