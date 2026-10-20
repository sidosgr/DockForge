namespace DockForge;

// Shared by the live bar, its animation snapshot and the Settings preview.
internal static class BarAppearance {
 static readonly ResourceDictionary XpResources = new() { Source = new Uri("/DockForge;component/Themes/WindowsXP.xaml", UriKind.Relative) };
 internal static bool IsXp(Bar bar) => bar.Style == BarStyle.WindowsXP && !SystemParameters.HighContrast;
 internal static bool Modern(Bar bar) => bar.Style == BarStyle.Windows11 || bar.Style == BarStyle.System && Environment.OSVersion.Version.Build >= 22000;
 internal static bool Blur(Bar bar) => bar.Style != BarStyle.WindowsXP && bar.Blur && !SystemParameters.HighContrast;
 internal static Brush Foreground(Bar bar) => SystemParameters.HighContrast ? SystemColors.WindowTextBrush : IsXp(bar) || Ui.Dark(bar) ? Brushes.White : Ui.Brush("#17202B", Brushes.Black);
 internal static CornerRadius Corners(Bar bar) => new(bar.CornerRadius >= 0 ? bar.CornerRadius : Modern(bar) ? 8 : 0);

 internal static LinearGradientBrush Gradient(Bar bar, params (string Color, double Offset)[] colors) {
  var brush = new LinearGradientBrush { StartPoint = new(0, 0), EndPoint = bar.IsVertical ? new(1, 0) : new(0, 1) };
  foreach (var color in colors) brush.GradientStops.Add(new((Color)ColorConverter.ConvertFromString(color.Color), color.Offset));
  brush.Freeze();
  return brush;
 }

 internal static Brush Background(Bar bar) {
  if (SystemParameters.HighContrast) return SystemColors.WindowBrush;
  Brush fallback = IsXp(bar) ? Gradient(bar, ("#1941A5", 0), ("#438FF4", .08), ("#2B76E8", .22), ("#245EDB", .62), ("#1941A5", 1)) : Ui.Dark(bar) ? Ui.Brush("#25272D", Brushes.Black) : Ui.Brush("#F5F6F8", Brushes.White);
  return Ui.Brush(bar.Background, fallback);
 }

 internal static void Paint(Border surface, Bar bar) {
  surface.Background = Background(bar);
  surface.BorderBrush = SystemParameters.HighContrast ? SystemColors.WindowTextBrush : IsXp(bar) ? Ui.Brush("#123D91", Brushes.Navy) : Ui.Dark(bar) ? Ui.Brush("#55565A", Brushes.Gray) : Ui.Brush("#D0D3D8", Brushes.Gray);
  surface.BorderThickness = new Thickness(1);
  surface.CornerRadius = Corners(bar);
 }

 internal static void PaintButton(Button button, Bar bar, bool running = false, bool active = false, bool hover = false) {
  button.Foreground = Foreground(bar);
  if (IsXp(bar)) {
   button.Style = (Style)XpResources["WindowsXPButton"];
   button.BorderThickness = new Thickness(1);
   button.BorderBrush = active ? Ui.Brush("#103A85", Brushes.Navy) : running || hover ? Ui.Brush("#79ACF9", Brushes.LightBlue) : Brushes.Transparent;
   button.Background = active ? Gradient(bar, ("#123E90", 0), ("#1C55B7", 1))
    : hover ? bar.HoverColor != "#26808080" ? Ui.Brush(bar.HoverColor, Brushes.CornflowerBlue) : Gradient(bar, ("#80B3FF", 0), ("#3E82EB", 1))
    : running ? Gradient(bar, ("#4B91F8", 0), ("#2862C8", 1)) : Brushes.Transparent;
  } else {
   button.ClearValue(FrameworkElement.StyleProperty);
   button.BorderThickness = new Thickness(0);
   button.Background = active || hover ? Ui.Brush(bar.HoverColor, Brushes.DimGray) : Brushes.Transparent;
  }
 }
}
