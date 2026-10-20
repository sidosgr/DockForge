using System.Text.Json;

namespace DockForge.Core;

public sealed record ReleaseUpdate(Version Version, string Tag, string DownloadUrl, bool IsPreview, string Sha256 = "", long Size = 0);

public static class UpdateFeed {
 public const string Repository = "sidosgr/DockForge";
 public const string ReleasesPage = "https://github.com/" + Repository + "/releases";
 public const string ApiUrl = "https://api.github.com/repos/" + Repository + "/releases?per_page=100";

 public static ReleaseUpdate? FindUpdate(string json, Version current, bool includePreviews, string architecture) {
  if (architecture is not ("x64" or "arm64")) throw new ArgumentException("Unsupported Windows architecture.", nameof(architecture));
  using var document = JsonDocument.Parse(json);
  if (document.RootElement.ValueKind != JsonValueKind.Array) throw new JsonException("Expected a GitHub releases list.");
  var candidates = new List<ReleaseUpdate>();
  foreach (var release in document.RootElement.EnumerateArray()) {
   if (Flag(release, "draft")) continue;
   bool preview = Flag(release, "prerelease");
   if (preview && !includePreviews) continue;
   string tag = Text(release, "tag_name");
   if (!Version.TryParse(tag.TrimStart('v', 'V'), out var version) || Normalize(version) <= Normalize(current)) continue;
   if (!release.TryGetProperty("assets", out var assets) || assets.ValueKind != JsonValueKind.Array) continue;
   string combinedName = $"DockForge-Setup-{version}.exe";
   // Prefer the combined installer while retaining compatibility with older architecture-specific releases.
   var compatible = assets.EnumerateArray().Where(a => Text(a, "name") == combinedName || Text(a, "name") == $"DockForge-Setup-{version}-{architecture}.exe")
    .OrderByDescending(a => Text(a, "name") == combinedName);
   foreach (var asset in compatible) {
    string name = Text(asset, "name"), url = Text(asset, "browser_download_url");
    string expected = $"https://github.com/{Repository}/releases/download/{Uri.EscapeDataString(tag)}/{Uri.EscapeDataString(name)}";
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !string.Equals(uri.AbsoluteUri, expected, StringComparison.Ordinal)) continue;
    string digest = Text(asset, "digest");
    string sha256 = digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) ? digest[7..] : "";
    long size = asset.TryGetProperty("size", out var sizeValue) && sizeValue.TryGetInt64(out var bytes) ? bytes : 0;
    candidates.Add(new(version, tag, url, preview, sha256, size));
    break;
   }
  }
  return candidates.OrderByDescending(c => Normalize(c.Version)).ThenBy(c => c.IsPreview).FirstOrDefault();
 }

 static Version Normalize(Version version) => new(version.Major, version.Minor, Math.Max(0, version.Build), Math.Max(0, version.Revision));
 static string Text(JsonElement element, string name) => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";
 static bool Flag(JsonElement element, string name) => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
}
