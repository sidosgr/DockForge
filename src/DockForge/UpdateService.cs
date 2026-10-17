using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace DockForge;

public static class UpdateService {
 public static Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
 public static string DisplayVersion => CurrentVersion.ToString(3);
 static bool checking;

 public static async Task Check(bool interactive) {
  if (checking) return;
  checking = true;
  try {
   using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
   client.DefaultRequestHeaders.UserAgent.ParseAdd($"DockForge/{DisplayVersion}");
   client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
   using var response = await client.GetAsync(UpdateFeed.ApiUrl);
   if (response.StatusCode == HttpStatusCode.NotFound) throw new InvalidDataException("The DockForge release repository is unavailable.");
   response.EnsureSuccessStatusCode();
   string architecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64 ? "arm64" : "x64";
   var update = UpdateFeed.FindUpdate(await response.Content.ReadAsStringAsync(), CurrentVersion, App.Host.Config.IncludePreviewUpdates, architecture);
   if (update is null) {
    if (interactive) MessageBox.Show($"No newer compatible {(App.Host.Config.IncludePreviewUpdates ? "release" : "stable release")} is available.\n\nInstalled version: {DisplayVersion}\nSource: {UpdateFeed.Repository}", "DockForge updates");
    return;
   }
   string kind = update.IsPreview ? "development preview" : "release";
   if (MessageBox.Show($"DockForge {update.Tag} ({kind}) is available from {UpdateFeed.Repository}.\n\nOpen the installer download in your browser? Run it when you are ready.", "DockForge update", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
    Process.Start(new ProcessStartInfo(update.DownloadUrl) { UseShellExecute = true });
  } catch (Exception error) {
   Log.Write(error.Message);
   if (interactive) MessageBox.Show("Could not check DockForge releases. " + error.Message, "DockForge updates");
  } finally { checking = false; }
 }
}
