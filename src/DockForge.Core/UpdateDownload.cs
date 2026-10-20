using System.Net.Http;
using System.Security.Cryptography;

namespace DockForge.Core;

public sealed record DownloadProgress(long Received, long Total);

public static class UpdateDownload {
 public const long MaximumSize = 512L * 1024 * 1024;

 public static void Validate(ReleaseUpdate release) {
  string name = Path.GetFileName(new Uri(release.DownloadUrl).AbsolutePath);
  string combined = $"DockForge-Setup-{release.Version}.exe";
  if (name != combined && name != $"DockForge-Setup-{release.Version}-x64.exe" && name != $"DockForge-Setup-{release.Version}-arm64.exe") throw new InvalidDataException("Unexpected installer name.");
  string expected = $"https://github.com/{UpdateFeed.Repository}/releases/download/{Uri.EscapeDataString(release.Tag)}/{name}";
  if (release.DownloadUrl != expected || release.Sha256.Length != 64 || !release.Sha256.All(Uri.IsHexDigit) || release.Size <= 0 || release.Size > MaximumSize) throw new InvalidDataException("Release verification metadata is unavailable.");
 }

 public static async Task<bool> VerifyAsync(string path, ReleaseUpdate release, CancellationToken token) {
  Validate(release);
  if (!File.Exists(path) || new FileInfo(path).Length != release.Size) return false;
  await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
  byte[] actual = await SHA256.HashDataAsync(file, token);
  return CryptographicOperations.FixedTimeEquals(actual, Convert.FromHexString(release.Sha256));
 }

 public static async Task MarkDownloadedAsync(string path, ReleaseUpdate release, CancellationToken token) {
  // Preserve Windows' normal handling of applications obtained from the Internet.
  if (OperatingSystem.IsWindows()) await File.WriteAllTextAsync(path + ":Zone.Identifier", $"[ZoneTransfer]\r\nZoneId=3\r\nHostUrl={release.DownloadUrl}\r\n", token);
 }

 public static async Task<string> DownloadAsync(HttpClient client, ReleaseUpdate release, string directory, IProgress<DownloadProgress>? progress, CancellationToken token) {
  Validate(release);
  Directory.CreateDirectory(directory);
  string destination = Path.Combine(directory, Path.GetFileName(new Uri(release.DownloadUrl).AbsolutePath));
  if (await VerifyAsync(destination, release, token)) {
   await MarkDownloadedAsync(destination, release, token);
   progress?.Report(new(release.Size, release.Size));
   return destination;
  }
  string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".partial";
  try {
   using var response = await client.GetAsync(release.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, token);
   response.EnsureSuccessStatusCode();
   var finalUri = response.RequestMessage?.RequestUri;
   if (finalUri is null || finalUri.Scheme != "https" || !(finalUri.Host == "github.com" || finalUri.Host.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase))) throw new InvalidDataException("Unexpected download destination.");
   if (response.Content.Headers.ContentLength is long length && length != release.Size) throw new InvalidDataException("Installer size does not match the release.");
   await using var source = await response.Content.ReadAsStreamAsync(token);
   using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
   long received = 0;
   long lastReport = Environment.TickCount64;
   await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true)) {
    byte[] buffer = new byte[81920];
    int count;
    while ((count = await source.ReadAsync(buffer, token)) > 0) {
     received += count;
     if (received > release.Size || received > MaximumSize) throw new InvalidDataException("Installer exceeds the expected size.");
     hash.AppendData(buffer, 0, count);
     await output.WriteAsync(buffer.AsMemory(0, count), token);
     if (received == count || Environment.TickCount64 - lastReport >= 100 || received == release.Size) { progress?.Report(new(received, release.Size)); lastReport = Environment.TickCount64; }
    }
    await output.FlushAsync(token);
   }
   if (received != release.Size || !CryptographicOperations.FixedTimeEquals(hash.GetHashAndReset(), Convert.FromHexString(release.Sha256))) throw new InvalidDataException("Installer checksum does not match the release.");
   token.ThrowIfCancellationRequested();
   File.Move(temporary, destination, true);
   await MarkDownloadedAsync(destination, release, token);
   return destination;
  } finally { if (File.Exists(temporary)) File.Delete(temporary); }
 }
}
