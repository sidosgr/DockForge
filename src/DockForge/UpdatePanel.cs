using System.Windows.Automation;

namespace DockForge;

internal sealed class UpdatePanel : Border {
 readonly UpdateClient client;
 readonly TextBlock status = Ui.Text("", 14);
 readonly TextBlock detail = Ui.Text("", 12);
 readonly ProgressBar progress = new() { Height = 5, Margin = new Thickness(0, 2, 0, 12) };
 readonly Button check, download, install, cancel;

 internal UpdatePanel(UpdateClient client, Func<bool> includePreviews, Func<Task> startInstaller) {
  this.client = client;
  Padding = new Thickness(16); Margin = new Thickness(0, 12, 0, 0); CornerRadius = new(8); VerticalAlignment = VerticalAlignment.Top;
  BorderBrush = Ui.Brush("#507D8590", Brushes.Gray); BorderThickness = new Thickness(1);
  var body = new StackPanel(); Child = body;
  body.Children.Add(Ui.Text($"Version {client.Version.ToString(3)} · MIT license", 15, true));
  body.Children.Add(Ui.Text("Official update source: " + UpdateFeed.Repository, 12));
  AutomationProperties.SetName(status, "Update status"); AutomationProperties.SetLiveSetting(status, AutomationLiveSetting.Polite);
  body.Children.Add(status); body.Children.Add(progress); body.Children.Add(detail);
  var actions = new WrapPanel();
  check = Ui.Action("Check now", async () => await client.CheckAsync(includePreviews()));
  download = Ui.Action("Download update", async () => await client.DownloadAsync());
  install = Ui.Action("Install update", async () => await startInstaller());
  cancel = Ui.Action("Cancel download", client.Cancel);
  foreach (var button in new[] { check, download, install, cancel }) actions.Children.Add(button);
  body.Children.Add(actions);
  Loaded += (_, _) => { client.Changed += Changed; Refresh(); };
  Unloaded += (_, _) => client.Changed -= Changed;
  Refresh();
 }

 void Changed(object? sender, EventArgs args) {
  if (Dispatcher.CheckAccess()) Refresh();
  else if (!Dispatcher.HasShutdownStarted) Dispatcher.BeginInvoke(Refresh);
 }

 void Refresh() {
  var state = client.State;
  status.Text = state.Message;
  if (state.IsError && !SystemParameters.HighContrast) status.Foreground = Ui.Dark(new Bar()) ? Brushes.LightCoral : Brushes.Firebrick;
  else status.ClearValue(TextBlock.ForegroundProperty);
  check.IsEnabled = !client.Busy && state.Phase != UpdatePhase.Launching;
  download.Visibility = state.Phase == UpdatePhase.Available ? Visibility.Visible : Visibility.Collapsed;
  install.Visibility = state.Phase == UpdatePhase.Ready ? Visibility.Visible : Visibility.Collapsed;
  download.IsEnabled = install.IsEnabled = !client.Busy;
  cancel.Visibility = state.Phase is UpdatePhase.Checking or UpdatePhase.Downloading ? Visibility.Visible : Visibility.Collapsed;
  cancel.Content = state.Phase == UpdatePhase.Checking ? "Cancel check" : "Cancel download";
  AutomationProperties.SetName(cancel, cancel.Content.ToString());
  progress.Visibility = state.Phase is UpdatePhase.Checking or UpdatePhase.Downloading or UpdatePhase.Verifying ? Visibility.Visible : Visibility.Collapsed;
  progress.IsIndeterminate = state.Phase != UpdatePhase.Downloading || state.Received == state.Total;
  progress.Maximum = Math.Max(1, state.Total); progress.Value = Math.Clamp(state.Received, 0, progress.Maximum);
  AutomationProperties.SetName(progress, state.Message);
  detail.Text = state.Phase == UpdatePhase.Ready ? "Download verified. Install update saves your settings and closes DockForge so setup can continue. Windows may ask you to allow the installer."
   : state.Phase == UpdatePhase.Downloading ? $"{state.Received / 1048576d:0.0} of {state.Total / 1048576d:0.0} MB"
   : state.Release is { } release ? $"Download size: {release.Size / 1048576d:0.0} MB. Download and installation start only when you choose them."
   : "Check results and download progress appear here. Updates do not install automatically.";
 }
}
