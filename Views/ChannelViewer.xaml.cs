using System.Windows.Controls;

namespace Illumine.LPR
{
    public partial class ChannelViewer : UserControl
    {
        public ChannelViewer()
        {
            InitializeComponent();
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //var vm = (ChannelViewerViewModel)this.DataContext;
            //if (vm == null)
            //    return;
            //vm.CameraViewModel.Stop();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //var vm = (ChannelViewerViewModel)this.DataContext;
            //if (vm == null)
            //    return;
            //vm.CameraViewModel.Play();
        }
    }
}
