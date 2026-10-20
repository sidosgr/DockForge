using System.ComponentModel;
using System.Net;
using System.Net.Http;

namespace DockForge.Core;

public enum UpdatePhase { Idle, Checking, Available, Downloading, Ready, Verifying, Launching, Error }
public sealed record UpdateState(UpdatePhase Phase, string Message, ReleaseUpdate? Release = null, long Received = 0, long Total = 0, bool IsError = false);

public sealed class UpdateClient(HttpClient http, string directory, Version version, string architecture) {
 public Version Version { get; } = version;
 public UpdateState State { get; private set; } = new(UpdatePhase.Idle, "Check for updates to DockForge.");
 public bool Busy => operation is not null;
 public event EventHandler? Changed;
 CancellationTokenSource? operation;
 bool cancelled;
 string? installer;

 void Set(UpdateState state) { State = state; Changed?.Invoke(this, EventArgs.Empty); }
 void Finish() { operation?.Dispose(); operation = null; Changed?.Invoke(this, EventArgs.Empty); }
 public void Cancel() { cancelled = true; operation?.Cancel(); }
 public void ReportProblem(string message) => Set(State with { Message = message, IsError = true });

 public async Task CheckAsync(bool includePreviews) {
  if (Busy) return;
  cancelled = false;
  operation = new(TimeSpan.FromSeconds(25));
  var token = operation.Token;
  Set(new(UpdatePhase.Checking, "Checking for updates…"));
  try {
   using var response = await http.GetAsync(UpdateFeed.ApiUrl, token);
   response.EnsureSuccessStatusCode();
   var release = UpdateFeed.FindUpdate(await response.Content.ReadAsStringAsync(token), Version, includePreviews, architecture);
   installer = null;
   if (release is null) Set(new(UpdatePhase.Idle, includePreviews ? "You’re up to date." : "No newer stable release is available."));
   else {
    UpdateDownload.Validate(release);
    Set(new(UpdatePhase.Available, $"DockForge {release.Tag} is available{(release.IsPreview ? " · Development preview" : "")}", release));
   }
  } catch (OperationCanceledException) { Set(new(UpdatePhase.Error, cancelled ? "Update check cancelled." : "Update check timed out. Try again.", IsError: !cancelled)); }
  catch (InvalidDataException) { Set(new(UpdatePhase.Error, "The release is missing valid verification details. Try again later.", IsError: true)); }
  catch (HttpRequestException error) { Set(new(UpdatePhase.Error, error.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests ? "GitHub has temporarily limited update checks. Try again later." : "Could not check for updates. Check your connection and try again.", IsError: true)); }
  catch (Exception) { Set(new(UpdatePhase.Error, "Could not read update information. Try again.", IsError: true)); }
  finally { Finish(); }
 }

 public async Task DownloadAsync() {
  if (Busy || State.Release is not { } release) return;
  cancelled = false;
  operation = new(TimeSpan.FromMinutes(10));
  var token = operation.Token;
  Set(new(UpdatePhase.Downloading, "Downloading update…", release, Total: release.Size));
  try {
   installer = await UpdateDownload.DownloadAsync(http, release, directory, new DirectProgress(p => Set(new(UpdatePhase.Downloading, p.Received == p.Total ? "Verifying download…" : $"Downloading update… {p.Received * 100 / p.Total}%", release, p.Received, p.Total))), token);
   Set(new(UpdatePhase.Ready, $"DockForge {release.Tag} is ready to install.", release, release.Size, release.Size));
  } catch (OperationCanceledException) { Set(new(UpdatePhase.Available, cancelled ? "Download cancelled. You can retry when ready." : "Download timed out. Try again.", release, IsError: !cancelled)); }
  catch (InvalidDataException) { installer = null; Set(new(UpdatePhase.Available, "Download verification failed. Please retry the download.", release, IsError: true)); }
  catch (HttpRequestException) { Set(new(UpdatePhase.Available, "Download failed. Check your connection and retry.", release, IsError: true)); }
  catch (Exception) { Set(new(UpdatePhase.Available, "Could not save the update. Check available disk space and retry.", release, IsError: true)); }
  finally { Finish(); }
 }

 public async Task<bool> InstallAsync(Func<string, Task> launch) {
  if (Busy || State.Phase != UpdatePhase.Ready || State.Release is not { } release || installer is null) return false;
  operation = new(TimeSpan.FromSeconds(30));
  var token = operation.Token;
  Set(new(UpdatePhase.Verifying, "Verifying installer…", release));
  try {
   if (!await UpdateDownload.VerifyAsync(installer, release, token)) { installer = null; Set(new(UpdatePhase.Available, "The downloaded installer changed. Download it again before installing.", release, IsError: true)); return false; }
   await UpdateDownload.MarkDownloadedAsync(installer, release, token);
   token.ThrowIfCancellationRequested();
   await launch(installer);
   Set(new(UpdatePhase.Launching, "Starting installer… DockForge will close.", release));
   return true;
  } catch (Exception error) {
   Set(new(UpdatePhase.Ready, error is Win32Exception { NativeErrorCode: 1223 } ? "Installation cancelled. You can try again when ready." : "The installer could not start. You can try again.", release, IsError: true));
   return false;
  } finally { Finish(); }
 }

 sealed class DirectProgress(Action<DownloadProgress> report) : IProgress<DownloadProgress> { public void Report(DownloadProgress value) => report(value); }
}
