using System.Diagnostics;
using System.Net.Http;
using System.Reflection;

namespace DockForge;

public static class UpdateService {
 public static Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
 public static string DisplayVersion => CurrentVersion.ToString(3);
 static UpdateClient? client;
 public static UpdateClient Client => client ??= CreateClient();

 static UpdateClient CreateClient() {
  var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
  http.DefaultRequestHeaders.UserAgent.ParseAdd($"DockForge/{DisplayVersion}");
  http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
  string architecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64 ? "arm64" : "x64";
  return new(http, Path.Combine(Log.Root, "Updates"), CurrentVersion, architecture);
 }

 public static void Cancel() => client?.Cancel();

 public static async Task InstallAsync() {
  bool started = await Client.InstallAsync(path => {
   using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
   if (process is null) throw new IOException("The installer did not start.");
   return Task.CompletedTask;
  });
  if (started) App.Host.Quit();
 }
}
