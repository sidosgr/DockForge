using System.Windows.Automation;
using System.Windows.Input;
namespace DockForge;
public static class Ui {
 public static TextBlock Text(string text,double size=14,bool bold=false)=>new(){Text=text,FontSize=size,FontWeight=bold?FontWeights.SemiBold:FontWeights.Normal,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,8)};
 public static Button Action(string text,Action action){var b=new Button{Content=text};AutomationProperties.SetName(b,text);b.Click+=(_,_)=>action();return b;}
 static StackPanel Field(Panel p,string label){var field=new StackPanel{Tag="Field",Margin=new Thickness(0)};Label(field,label);p.Children.Add(field);return field;}
 public static void Label(Panel p,string text){var label=Text(text,13,true);label.Margin=new Thickness(0,0,0,4);p.Children.Add(label);}
 public static TextBox Input(Panel p,string label,string value,Action<string> change){var field=Field(p,label);var box=new TextBox{Text=value};AutomationProperties.SetName(box,label);box.TextChanged+=(_,_)=>change(box.Text);field.Children.Add(box);return box;}
 public static CheckBox Check(Panel p,string label,bool value,Action<bool> change){var c=new CheckBox{Content=new TextBlock{Text=label,TextWrapping=TextWrapping.Wrap},IsChecked=value,Tag="Field"};c.Checked+=(_,_)=>change(true);c.Unchecked+=(_,_)=>change(false);p.Children.Add(c);return c;}
 public static string Friendly(object value)=>System.Text.RegularExpressions.Regex.Replace(value.ToString()??"",@"(?<=[a-z])(?=[A-Z0-9])"," ");
 public static ComboBox Choice<T>(Panel p,string label,T value,Action<T> change) where T:struct,Enum{var field=Field(p,label);var c=new ComboBox{ItemsSource=Enum.GetValues<T>().Select(v=>new KeyValuePair<string,T>(Friendly(v),v)),DisplayMemberPath="Key",SelectedValuePath="Value",SelectedValue=value};AutomationProperties.SetName(c,label);c.SelectionChanged+=(_,_)=>{if(c.SelectedValue is T v)change(v);};field.Children.Add(c);return c;}
 public static TextBox Number(Panel p,string label,double value,double min,double max,Action<double> change){return Input(p,label,value.ToString(System.Globalization.CultureInfo.InvariantCulture),s=>{if(double.TryParse(s,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var n)&&double.IsFinite(n)&&n>=min&&n<=max)change(n);});}
 public static Brush Brush(string text,Brush fallback){try{return (Brush)new BrushConverter().ConvertFromString(text)!;}catch{return fallback;}}
 public static bool Dark(Bar bar)=>bar.Theme==ThemeMode.Dark||(bar.Theme==ThemeMode.System&&Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize","AppsUseLightTheme",1) is int v&&v==0);
 public static void Menu(ItemsControl menu,string text,Action action,bool enabled=true){var i=new MenuItem{Header=text,IsEnabled=enabled};i.Click+=(_,_)=>action();menu.Items.Add(i);}
 public static string? File(bool icons=false){var d=new Microsoft.Win32.OpenFileDialog{Filter=icons?"Images and icons|*.ico;*.png;*.jpg;*.jpeg;*.bmp;*.exe;*.dll|All files|*.*":"All files|*.*"};return d.ShowDialog()==true?d.FileName:null;}
 public static string? Folder(){var d=new Microsoft.Win32.OpenFolderDialog();return d.ShowDialog()==true?d.FolderName:null;}
 public static void Theme(Window window,Bar bar){bool dark=Dark(bar);window.Background=SystemParameters.HighContrast?SystemColors.WindowBrush:dark?Brush("#25272D",Brushes.Black):Brush("#F5F6F8",Brushes.White);window.Foreground=SystemParameters.HighContrast?SystemColors.WindowTextBrush:dark?Brushes.White:Brush("#17202B",Brushes.Black);}
 public static void ArrowNavigation(UIElement e){e.PreviewKeyDown+=(_,a)=>{FocusNavigationDirection? dir=a.Key switch{Key.Right=>FocusNavigationDirection.Next,Key.Down=>FocusNavigationDirection.Next,Key.Left=>FocusNavigationDirection.Previous,Key.Up=>FocusNavigationDirection.Previous,_=>null};if(dir!=null&&Keyboard.FocusedElement is UIElement element){element.MoveFocus(new TraversalRequest(dir.Value));a.Handled=true;}};}
}
