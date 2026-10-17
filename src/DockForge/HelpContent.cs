using System.Diagnostics;

namespace DockForge;

public static class HelpContent {
 public static FrameworkElement QuickStart(){
  var body=new StackPanel();body.Children.Add(Ui.Text("A little guidance. More room to work.",26,true));
  body.Children.Add(Ui.Text("The essentials for building a workspace that feels like yours.",14));
  var open=Ui.Action("Open the complete illustrated guide ↗",OpenGuide);open.HorizontalAlignment=HorizontalAlignment.Left;open.Margin=new Thickness(0,8,0,22);body.Children.Add(open);
  var cards=new ResponsiveForm();
  void Card(string number,string title,string text){var content=new StackPanel();content.Children.Add(new TextBlock{Text=number,Foreground=Ui.Brush("#60CDFF",Brushes.DeepSkyBlue),FontSize=12,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,8)});content.Children.Add(Ui.Text(title,18,true));content.Children.Add(Ui.Text(text,14));var border=new Border{Tag="Field",Padding=new Thickness(18),Margin=new Thickness(0,0,0,10),CornerRadius=new CornerRadius(8),BorderThickness=new Thickness(1),BorderBrush=Ui.Brush("#507D8590",Brushes.Gray),Child=content};cards.Children.Add(border);}
  Card("01 / BUILD","Add a bar","Choose New bar, then select a display and an edge or floating position. Choose Launcher, Running apps, or Combined. Select several displays to mirror one bar.");
  Card("02 / COLLECT","Add your essentials","Right-click an empty part of a bar → New. Add apps, files, folders, websites or groups. Drag icons to reorder; the accent line shows where they will land.");
  Card("03 / GROUP","Keep related things together","Use a custom group for selected items or a linked folder for a live folder view. Right-click its heading to rename it, hide item labels or hide shortcut badges.");
  Card("04 / TUNE","Get the size right","Changing icon size resets bar thickness to the smallest comfortable size. You can increase thickness afterward. Alignment centers the icons inside full-length bars too.");
  Card("05 / SWITCH","Work with running apps","Click an app to switch to it. Hover to inspect its windows; move away from the icon to dismiss. Right-click for window actions. Middle-click closes by default; configure it per bar.");
  Card("06 / RECOVER","Stay in control","Apply saves changes; Cancel reverts unsaved edits. Use Backups & updates to export your setup. Restore Windows taskbars is available in Settings, the bar menu and the notification-area menu.");
  body.Children.Add(cards);body.Children.Add(Ui.Text("Auto-hide tip: move to the bar’s own screen edge to reveal it. The activation guide is visible only while Settings is open. Press Escape to close a group.",13));
  return new ScrollViewer{Content=body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled,Padding=new Thickness(16)};
 }
 public static void OpenGuide(){string path=Path.Combine(AppContext.BaseDirectory,"Help.html");try{if(!File.Exists(path))throw new FileNotFoundException("The complete guide is missing. Reinstall DockForge to restore it.");Process.Start(new ProcessStartInfo(path){UseShellExecute=true});}catch(Exception e){MessageBox.Show(e.Message,"Could not open the guide");}}
}
