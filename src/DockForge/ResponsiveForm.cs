namespace DockForge;

// Keep label/control pairs together and reflow at narrow window sizes or high DPI.
public sealed class ResponsiveForm : Panel {
 const double Gap=24;
 readonly Dictionary<UIElement,Rect> slots=[];
 double Layout(double width,bool measure){
  slots.Clear();bool columns=width>=620;double cell=columns?(width-Gap)/2:width;
  double y=0,rowHeight=0;int column=0;
  foreach(UIElement child in InternalChildren){
   bool field=child is FrameworkElement f&&Equals(f.Tag,"Field");
   if(!field&&column>0){y+=rowHeight+8;column=0;rowHeight=0;}
   double w=field?cell:width;
   if(measure)child.Measure(new Size(w,double.PositiveInfinity));
   double h=child.DesiredSize.Height;
   slots[child]=new Rect(column==0?0:cell+Gap,y,w,h);
   if(field&&columns){rowHeight=Math.Max(rowHeight,h);if(++column==2){y+=rowHeight+8;column=0;rowHeight=0;}}
   else y+=h+8;
  }
  return y+(column>0?rowHeight:0);
 }
 protected override Size MeasureOverride(Size available){double width=double.IsInfinity(available.Width)?700:available.Width;return new Size(width,Layout(width,true));}
 protected override Size ArrangeOverride(Size final){Layout(final.Width,false);foreach(var pair in slots)pair.Key.Arrange(pair.Value);return final;}
}
