using DockForge.Core;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

internal static class UpdateScenarios {
 public static async Task Run(Action<bool, string> check) {
  string root = Path.Combine(Path.GetTempPath(), "DockForge-update-tests-" + Guid.NewGuid().ToString("N"));
  byte[] payload = new byte[180000]; new Random(42).NextBytes(payload); payload[0] = (byte)'M'; payload[1] = (byte)'Z';
  string digest = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
  const string downloadUrl = "https://github.com/sidosgr/DockForge/releases/download/v2.0.0/DockForge-Setup-2.0.0.exe";
  string Feed(string? hash = null, long? size = null) => JsonSerializer.Serialize(new[] { new { tag_name = "v2.0.0", prerelease = false, draft = false, assets = new[] { new { name = "DockForge-Setup-2.0.0.exe", browser_download_url = downloadUrl, digest = "sha256:" + (hash ?? digest), size = size ?? payload.Length } } } });
  try {
   using var handler = new FixtureHandler(Feed(), payload);
   using var http = new HttpClient(handler);
   var client = new UpdateClient(http, Path.Combine(root, "valid"), new(1, 0, 0), "x64");
   var phases = new List<UpdatePhase>(); client.Changed += (_, _) => phases.Add(client.State.Phase);
   await client.CheckAsync(true);
   check(client.State.Phase == UpdatePhase.Available && phases.Contains(UpdatePhase.Checking), "Update check publishes inline checking and availability states");
   check(client.State.Release is { Sha256: var sha, Size: 180000 } && sha == digest, "Official asset checksum and size are retained for download verification");
   await client.DownloadAsync();
   string path = Path.Combine(root, "valid", "DockForge-Setup-2.0.0.exe");
   check(client.State.Phase == UpdatePhase.Ready && File.ReadAllBytes(path).SequenceEqual(payload), "In-app download reaches Ready only after verified bytes are saved");
   check(client.State.Received == payload.Length && phases.Contains(UpdatePhase.Downloading), "In-app download reports byte progress");
   check(!Directory.GetFiles(Path.Combine(root, "valid"), "*.partial").Any(), "Successful download leaves no partial files");
   if (OperatingSystem.IsWindows()) check(File.ReadAllText(path + ":Zone.Identifier").Contains("ZoneId=3"), "Downloaded installer retains Windows Internet-zone security handling");
   int downloads = handler.Downloads;
   var cached = new UpdateClient(http, Path.Combine(root, "valid"), new(1, 0, 0), "x64");
   await cached.CheckAsync(true); await cached.DownloadAsync();
   check(cached.State.Phase == UpdatePhase.Ready && handler.Downloads == downloads, "A verified cached installer is reused without downloading again");
   int launches = 0;
   bool started = await client.InstallAsync(file => { check(file == path, "Installer handoff uses the verified local file"); launches++; return Task.CompletedTask; });
   check(started && launches == 1 && client.State.Phase == UpdatePhase.Launching, "Explicit install hands off once after a second checksum verification");
   payload[20] ^= 1; File.WriteAllBytes(path, payload); payload[20] ^= 1;
   started = await cached.InstallAsync(_ => { launches++; return Task.CompletedTask; });
   check(!started && launches == 1 && cached.State.Phase == UpdatePhase.Available, "Tampering after download blocks installer execution");

   using var badHandler = new FixtureHandler(Feed(new string('0', 64)), payload);
   using var badHttp = new HttpClient(badHandler);
   var bad = new UpdateClient(badHttp, Path.Combine(root, "bad"), new(1, 0, 0), "x64");
   await bad.CheckAsync(true); await bad.DownloadAsync();
   check(bad.State is { Phase: UpdatePhase.Available, IsError: true } && !Directory.GetFiles(Path.Combine(root, "bad")).Any(), "Checksum mismatch reports inline failure and discards the partial download");

   using var shortHandler = new FixtureHandler(Feed(), payload[..10]);
   using var shortHttp = new HttpClient(shortHandler);
   var shortClient = new UpdateClient(shortHttp, Path.Combine(root, "short"), new(1, 0, 0), "x64");
   await shortClient.CheckAsync(true); await shortClient.DownloadAsync();
   check(shortClient.State.IsError && shortClient.State.Phase != UpdatePhase.Ready, "Truncated downloads never become installable");

   using var cancelHandler = new FixtureHandler(Feed(), payload) { BlockDownload = true };
   using var cancelHttp = new HttpClient(cancelHandler);
   var cancellable = new UpdateClient(cancelHttp, Path.Combine(root, "cancel"), new(1, 0, 0), "x64");
   await cancellable.CheckAsync(true);
   var pending = cancellable.DownloadAsync(); cancellable.Cancel(); await pending;
   check(cancellable.State is { Phase: UpdatePhase.Available, IsError: false } && !Directory.GetFiles(Path.Combine(root, "cancel")).Any(), "Cancelling a download restores retry controls and removes partial data");
   cancelHandler.BlockDownload = false; await cancellable.DownloadAsync();
   check(cancellable.State.Phase == UpdatePhase.Ready, "A cancelled download can be retried successfully");
   bool cancelledInstall = await cancellable.InstallAsync(_ => throw new Win32Exception(1223));
   check(!cancelledInstall && cancellable.State.Phase == UpdatePhase.Ready && cancellable.State.Message.Contains("cancelled"), "Declining Windows installer launch keeps the app ready to retry");

   using var checkHandler = new FixtureHandler(Feed(), payload) { BlockCheck = true };
   using var checkHttp = new HttpClient(checkHandler);
   var checkClient = new UpdateClient(checkHttp, Path.Combine(root, "checking"), new(1, 0, 0), "x64");
   var checking = checkClient.CheckAsync(true); await checkClient.CheckAsync(true); checkClient.Cancel(); await checking;
   check(checkHandler.Checks == 1 && !checkClient.Busy && checkClient.State.Message.Contains("cancelled"), "Duplicate checks are suppressed and checking can be cancelled inline");

   using var noChecksumHandler = new FixtureHandler(Feed(""), payload);
   using var noChecksumHttp = new HttpClient(noChecksumHandler);
   var noChecksum = new UpdateClient(noChecksumHttp, Path.Combine(root, "no-checksum"), new(1, 0, 0), "x64");
   await noChecksum.CheckAsync(true);
   check(noChecksum.State.Phase == UpdatePhase.Error && noChecksumHandler.Downloads == 0, "Releases without verification metadata cannot be downloaded or installed");

   using var redirectHandler = new FixtureHandler(Feed(), payload) { RedirectOutsideGitHub = true };
   using var redirectHttp = new HttpClient(redirectHandler);
   var redirected = new UpdateClient(redirectHttp, Path.Combine(root, "redirect"), new(1, 0, 0), "x64");
   await redirected.CheckAsync(true); await redirected.DownloadAsync();
   check(redirected.State.IsError && redirected.State.Phase != UpdatePhase.Ready, "A redirect outside GitHub download hosts is rejected");

   using var oversizedHandler = new FixtureHandler(Feed(size: UpdateDownload.MaximumSize + 1), payload);
   using var oversizedHttp = new HttpClient(oversizedHandler);
   var oversized = new UpdateClient(oversizedHttp, Path.Combine(root, "oversized"), new(1, 0, 0), "x64");
   await oversized.CheckAsync(true);
   check(oversized.State.Phase == UpdatePhase.Error && oversizedHandler.Downloads == 0, "Oversized installer metadata is rejected before downloading");
  } finally {
   string full = Path.GetFullPath(root);
   if (!full.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(full).StartsWith("DockForge-update-tests-")) throw new IOException("Unexpected test cleanup path.");
   if (Directory.Exists(full)) Directory.Delete(full, true);
  }
 }

 sealed class FixtureHandler(string feed, byte[] payload) : HttpMessageHandler {
  public int Checks, Downloads;
  public bool BlockCheck, BlockDownload, RedirectOutsideGitHub;
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) {
   bool isCheck = request.RequestUri!.Host == "api.github.com";
   if (isCheck) { Checks++; if (BlockCheck) await Task.Delay(Timeout.Infinite, token); }
   else Downloads++;
   return new(HttpStatusCode.OK) {
    RequestMessage = RedirectOutsideGitHub && !isCheck ? new(HttpMethod.Get, "https://example.com/installer.exe") : request,
    Content = isCheck ? new StringContent(feed) : BlockDownload ? new StreamContent(new WaitingStream()) : new ByteArrayContent(payload)
   };
  }
 }

 sealed class WaitingStream : Stream {
  public override bool CanRead => true; public override bool CanSeek => false; public override bool CanWrite => false;
  public override long Length => throw new NotSupportedException(); public override long Position { get => 0; set => throw new NotSupportedException(); }
  public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default) { await Task.Delay(Timeout.Infinite, token); return 0; }
  public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
  public override void Flush() => throw new NotSupportedException(); public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
  public override void SetLength(long value) => throw new NotSupportedException(); public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
 }
}
