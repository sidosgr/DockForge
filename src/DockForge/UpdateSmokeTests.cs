using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Automation;
using System.Windows.Media.Imaging;

namespace DockForge;

internal static class UpdateSmokeTests {
 static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject {
  for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++) {
   var child = VisualTreeHelper.GetChild(root, index);
   if (child is T match) yield return match;
   foreach (var descendant in Find<T>(child)) yield return descendant;
  }
 }

 internal static async Task Run(string output, Action<bool, string> check) {
  byte[] bytes = new byte[120000]; new Random(7).NextBytes(bytes); bytes[0] = (byte)'M'; bytes[1] = (byte)'Z';
  using var fixture = new UpdateFixture(bytes);
  using var http = new HttpClient(fixture);
  string cache = Path.Combine(Path.GetTempPath(), "DockForge-update-ui-" + Guid.NewGuid().ToString("N"));
  var client = new UpdateClient(http, cache, UpdateService.CurrentVersion, "x64");
  int launches = 0;
  async Task InstallFixture() => await client.InstallAsync(_ => { launches++; return Task.CompletedTask; });
  var panel = new UpdatePanel(client, () => true, InstallFixture);
  var host = new Window { Title = "DockForge update panel verification", Width = 760, Height = 480, ShowInTaskbar = false, Topmost = true, Content = new Border { Padding = new Thickness(20), Child = panel } };
  Ui.Theme(host, new Bar()); host.Show(); await Task.Delay(100);
  try {
   Button ButtonNamed(string name) => Find<Button>(host).Single(button => Equals(button.Content, name));
   string Status() => Find<TextBlock>(host).Single(text => AutomationProperties.GetName(text) == "Update status").Text;
   async Task Finish() { var until = DateTime.UtcNow.AddSeconds(5); while (client.Busy && DateTime.UtcNow < until) await Task.Delay(10); if (client.Busy) throw new TimeoutException("Update fixture did not finish."); host.UpdateLayout(); }
   void Capture(string name) {
    host.UpdateLayout(); var content = (FrameworkElement)host.Content;
    if (content is Border border) border.Background = host.Background;
    var bitmap = new RenderTargetBitmap((int)Math.Ceiling(content.ActualWidth), (int)Math.Ceiling(content.ActualHeight), 96, 96, PixelFormats.Pbgra32);
    bitmap.Render(content); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
    using var stream = File.Create(Path.Combine(output, name)); encoder.Save(stream);
   }
   ButtonNamed("Check now").RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); await Task.Delay(50);
   check(client.State.Phase == UpdatePhase.Checking && Status().Contains("Checking") && !ButtonNamed("Check now").IsEnabled, "Checking appears inline beside the version with duplicate checks disabled"); Capture("update-checking.png");
   fixture.CheckGate.TrySetResult(true); await Finish();
   check(Status().Contains("v9.9.9") && ButtonNamed("Download update").IsVisible && !ButtonNamed("Install update").IsVisible, "Available update exposes an in-app download action"); Capture("update-available.png");
   ButtonNamed("Download update").RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); await Task.Delay(100);
   check(client.State.Phase == UpdatePhase.Downloading && client.State.Received > 0 && Find<ProgressBar>(host).Single().IsVisible, "Downloading displays byte progress in the version card"); Capture("update-downloading.png");
   ButtonNamed("Cancel download").RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); await Finish();
   check(Status().Contains("cancelled") && ButtonNamed("Download update").IsVisible, "Cancelling restores the inline download action");
   fixture.BlockDownload = false; ButtonNamed("Download update").RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); await Finish();
   check(client.State.Phase == UpdatePhase.Ready && ButtonNamed("Install update").IsVisible && launches == 0, "Verified download offers installation without launching automatically"); Capture("update-ready.png");
   host.Content = new Border { Padding = new Thickness(20), Child = new UpdatePanel(client, () => true, InstallFixture) }; await Task.Delay(50);
   check(ButtonNamed("Install update").IsVisible, "Reopening the version panel preserves the verified download");
   ButtonNamed("Install update").RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); await Finish();
   check(launches == 1 && client.State.Phase == UpdatePhase.Launching, "Install button invokes the verified installer handoff once");
   fixture.EmptyFeed = true; await client.CheckAsync(true);
   check(Status().Contains("up to date") && ButtonNamed("Check now").IsEnabled, "Up-to-date result stays in the version card");
   fixture.FailCheck = true; await client.CheckAsync(true);
   check(client.State.Phase == UpdatePhase.Error && Status().Contains("Could not check") && ButtonNamed("Check now").IsEnabled, "Network failures stay inline and allow retry"); Capture("update-error.png");
  } finally {
   host.Close();
   if (Directory.Exists(cache) && Path.GetFileName(cache).StartsWith("DockForge-update-ui-")) Directory.Delete(cache, true);
  }
 }

 sealed class UpdateFixture(byte[] bytes) : HttpMessageHandler {
  public readonly TaskCompletionSource<bool> CheckGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
  public bool BlockDownload = true, EmptyFeed, FailCheck;
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) {
   if (request.RequestUri!.Host == "api.github.com") {
    await CheckGate.Task.WaitAsync(token);
    if (FailCheck) throw new HttpRequestException("Fixture network failure");
    string json = EmptyFeed ? "[]" : JsonSerializer.Serialize(new[] { new { tag_name = "v9.9.9", prerelease = true, draft = false, assets = new[] { new { name = "DockForge-Setup-9.9.9.exe", browser_download_url = "https://github.com/sidosgr/DockForge/releases/download/v9.9.9/DockForge-Setup-9.9.9.exe", digest = "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), size = bytes.Length } } } });
    return new(HttpStatusCode.OK) { RequestMessage = request, Content = new StringContent(json) };
   }
   HttpContent content = BlockDownload ? new StreamContent(new UpdateFixtureStream(bytes)) : new ByteArrayContent(bytes);
   content.Headers.ContentLength = bytes.Length;
   return new(HttpStatusCode.OK) { RequestMessage = request, Content = content };
  }
 }

 sealed class UpdateFixtureStream(byte[] bytes) : Stream {
  bool sent;
  public override bool CanRead => true; public override bool CanSeek => false; public override bool CanWrite => false;
  public override long Length => throw new NotSupportedException(); public override long Position { get => 0; set => throw new NotSupportedException(); }
  public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default) {
   if (!sent) { sent = true; int count = Math.Min(40000, buffer.Length); bytes.AsMemory(0, count).CopyTo(buffer); return count; }
   await Task.Delay(Timeout.Infinite, token); return 0;
  }
  public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
  public override void Flush() => throw new NotSupportedException(); public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
  public override void SetLength(long value) => throw new NotSupportedException(); public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
 }
}
