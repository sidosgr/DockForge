using System.Net.Http;
using System.Text.Json;
using System.Diagnostics;
namespace DockForge;
public static class UpdateService {
 public static async Task Check(string repository,bool interactive){try{
  if(!System.Text.RegularExpressions.Regex.IsMatch(repository,@"^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$")){if(interactive)MessageBox.Show("Enter the GitHub repository as owner/name in Settings.","Updates");return;}
  using var client=new HttpClient{Timeout=TimeSpan.FromSeconds(15)};client.DefaultRequestHeaders.UserAgent.ParseAdd("DockForge/0.1.4");using var doc=JsonDocument.Parse(await client.GetStringAsync($"https://api.github.com/repos/{repository}/releases/latest"));var root=doc.RootElement;var tag=root.GetProperty("tag_name").GetString()??"";
  if(!Version.TryParse(tag.TrimStart('v'),out var next)||next<=new Version(0,1,4)){if(interactive)MessageBox.Show("DockForge is up to date.","Updates");return;}
  string arch=System.Runtime.InteropServices.RuntimeInformation.OSArchitecture==System.Runtime.InteropServices.Architecture.Arm64?"arm64":"x64";
  var assets=root.GetProperty("assets").EnumerateArray().ToList();var asset=assets.FirstOrDefault(a=>string.Equals(a.GetProperty("name").GetString(),$"DockForge-Setup-{next}.exe",StringComparison.OrdinalIgnoreCase));if(asset.ValueKind==JsonValueKind.Undefined)asset=assets.FirstOrDefault(a=>(a.GetProperty("name").GetString()??"").EndsWith($"{arch}.exe",StringComparison.OrdinalIgnoreCase));
  var url=asset.ValueKind==JsonValueKind.Undefined?root.GetProperty("html_url").GetString():asset.GetProperty("browser_download_url").GetString();
  if(url==null||!Uri.TryCreate(url,UriKind.Absolute,out var uri)||uri.Scheme!="https"||uri.Host!="github.com")throw new InvalidDataException("Unexpected update address.");
  if(MessageBox.Show($"DockForge {tag} is available.\n\nOpen the installer download in your browser? Run the downloaded installer when you are ready.","DockForge update",MessageBoxButton.YesNo)==MessageBoxResult.Yes)Process.Start(new ProcessStartInfo(url){UseShellExecute=true});
 }catch(Exception e){Log.Write(e.Message);if(interactive)MessageBox.Show("Could not check for updates. "+e.Message,"Updates");}}
}
