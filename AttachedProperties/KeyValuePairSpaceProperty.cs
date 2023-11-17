using System.Windows;
using System.Windows.Controls;

namespace Illumine.LPR
{
    public class KeyValuePairSpaceProperty : BaseAttachedProperty<KeyValuePairSpaceProperty, int>
    {
        public override void OnValueChanged(
          DependencyObject sender,
          DependencyPropertyChangedEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null)
                return;
            int space = (int)e.NewValue;
            this.SetSpace(panel, space);
            RoutedEventHandler onLoaded = (RoutedEventHandler)null;
            onLoaded = (RoutedEventHandler)((s, ee) =>
           {
               panel.Loaded -= onLoaded;
               this.SetSpace(panel, space);
               foreach (object child in panel.Children)
               {
                   if (child is KeyValuePairControl valuePairControl2)
                       valuePairControl2.Label.SizeChanged += (SizeChangedEventHandler)((ss, eee) => this.SetSpace(panel, space));
               }
           });
            panel.Loaded += onLoaded;
        }

        private void SetSpace(Panel panel, int space)
        {
            Thickness thickness = (Thickness)new ThicknessConverter().ConvertFromString(string.Format("0 {0} 0 {1}", (object)space, (object)space));
            foreach (object child in panel.Children)
            {
                if (child is KeyValuePairControl valuePairControl)
                    valuePairControl.Margin = thickness;
            }
        }
    }
}
