using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DockForge;

// The whole moving frame is rendered by WPF. There are no per-frame HWND
// regions or backdrop changes to race against the retained visual tree.
internal sealed class BarAnimationWindow : Window {
 readonly Grid viewport=new(){ClipToBounds=true};
 readonly TranslateTransform offset=new();
 readonly Edge edge;
 readonly double frameWidth,frameHeight;
 public bool Ready {get;private set;}
 internal FrameworkElement Frame=>viewport;

 public BarAnimationWindow(FrameworkElement source,Bar model,Monitor monitor,Bounds bounds,double progress){
  edge=model.Edge;frameWidth=bounds.Width/monitor.Scale;frameHeight=bounds.Height/monitor.Scale;
  Title="DockForge animation";WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;
  AllowsTransparency=true;Background=Brushes.Transparent;ShowInTaskbar=App.Host.UiTest;ShowActivated=false;Topmost=true;
  Width=frameWidth;Height=frameHeight;Opacity=SystemParameters.HighContrast?1:Math.Clamp(model.Opacity,.2,1);
  source.UpdateLayout();
  var bitmap=new RenderTargetBitmap(Math.Max(1,(int)Math.Ceiling(bounds.Width)),Math.Max(1,(int)Math.Ceiling(bounds.Height)),96*monitor.Scale,96*monitor.Scale,PixelFormats.Pbgra32);
  bitmap.Render(source);bitmap.Freeze();
  var frame=new Image{Source=bitmap,Width=frameWidth,Height=frameHeight,Stretch=Stretch.Fill,RenderTransform=offset,HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top};
  viewport.Children.Add(frame);Content=viewport;SetProgress(progress);
  SourceInitialized+=(_,_)=>{var handle=new WindowInteropHelper(this).Handle;Native.SetWindowLongPtr(handle,-20,(nint)((long)Native.GetWindowLongPtr(handle,-20)|0x08000020|0x80));Native.SetWindowPos(handle,(nint)(-1),(int)Math.Round(bounds.X),(int)Math.Round(bounds.Y),(int)Math.Round(bounds.Width),(int)Math.Round(bounds.Height),0x10);};
  ContentRendered+=(_,_)=>Ready=true;
 }
 public void SetProgress(double progress){
  var position=Layout.RevealOffset(edge,frameWidth,frameHeight,progress);offset.X=position.X;offset.Y=position.Y;
  viewport.Opacity=edge==Edge.Floating?Math.Clamp(progress,0,1):1;
 }
}
