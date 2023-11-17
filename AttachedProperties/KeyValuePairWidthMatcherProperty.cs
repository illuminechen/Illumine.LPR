using System.Windows;
using System.Windows.Controls;

namespace Illumine.LPR
{
    public class KeyValuePairWidthMatcherProperty :
      BaseAttachedProperty<KeyValuePairWidthMatcherProperty, int>
    {
        public override void OnValueChanged(
          DependencyObject sender,
          DependencyPropertyChangedEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null)
                return;
            int width = (int)e.NewValue;
            this.SetWidths(panel, width);
            RoutedEventHandler onLoaded = null;
            onLoaded = (s, ee) =>
           {
               panel.Loaded -= onLoaded;
               this.SetWidths(panel, width);
               foreach (object child in panel.Children)
               {
                   if (child is KeyValuePairControl valuePairControl2)
                       valuePairControl2.Label.SizeChanged += (ss, eee) => this.SetWidths(panel, width);
               }
           };
            panel.Loaded += onLoaded;
        }

        private void SetWidths(Panel panel, int width)
        {
            GridLength gridLength = (GridLength)new GridLengthConverter().ConvertFromString(width.ToString());
            foreach (object child in panel.Children)
            {
                if (child is KeyValuePairControl valuePairControl)
                    valuePairControl.LabelWidth = gridLength;
            }
        }
    }
}
