using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;

namespace Illumine.LPR
{
    public partial class CameraViewer : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty OpenDoorCommandProperty = DependencyProperty.Register(nameof(OpenDoorCommand), typeof(ICommand), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(CameraViewer.OpenDoorCommandChanged)));
        public static readonly DependencyProperty PlayingCameraIdProperty = DependencyProperty.Register(nameof(PlayingCameraId), typeof(IntPtr), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(CameraViewer.PlayingCameraIdChanged)));
        public static readonly DependencyProperty ModeTextProperty = DependencyProperty.Register(nameof(ModeText), typeof(string), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(CameraViewer.ModeTextChanged)));
        public static readonly DependencyProperty EntryTextProperty = DependencyProperty.Register(nameof(EntryText), typeof(string), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(CameraViewer.EntryTextChanged)));
        public static readonly DependencyProperty TimeTextProperty = DependencyProperty.Register(nameof(TimeText), typeof(string), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(CameraViewer.TimeTextChanged)));
        public static readonly DependencyProperty PresentModeProperty = DependencyProperty.Register(nameof(PresentMode), typeof(PresentMode), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(CameraViewer.PresentModeChanged)));
        public static readonly DependencyProperty SnapshotIageProperty = DependencyProperty.Register(nameof(SnapshotImage), typeof(System.Drawing.Image), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(CameraViewer.SnapshotImageChanged)));
        public static readonly DependencyProperty EtagConnectingProperty = DependencyProperty.Register(nameof(EtagConnecting), typeof(bool), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(CameraViewer.EtagConnectingChanged)));
        public static readonly DependencyProperty ValidEtagProperty = DependencyProperty.Register(nameof(ValidEtag), typeof(bool), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(CameraViewer.ValidEtagChanged)));
        public static readonly DependencyProperty ETagNumberProperty = DependencyProperty.Register(nameof(ETagNumber), typeof(string), typeof(CameraViewer), new PropertyMetadata(new PropertyChangedCallback(ETagNumberChanged)));


        private Camera Camera => Container.Get<Camera>(((CameraViewerViewModel)this.DataContext)?.ChannelId ?? 0);

        public CameraViewer()
        {
            InitializeComponent();
        }

        public string ETagNumber
        {
            get { return (string)GetValue(ETagNumberProperty); }
            set { SetValue(ETagNumberProperty, value); }
        }

        public bool ValidEtag
        {
            get { return (bool)GetValue(ValidEtagProperty); }
            set { SetValue(ValidEtagProperty, value); }
        }

        public bool EtagConnecting
        {
            get { return (bool)GetValue(EtagConnectingProperty); }
            set { SetValue(EtagConnectingProperty, value); }
        }

        public ICommand OpenDoorCommand
        {
            get => (ICommand)this.GetValue(OpenDoorCommandProperty);
            set => this.SetValue(OpenDoorCommandProperty, value);
        }

        public IntPtr PlayingCameraId
        {
            get => (IntPtr)this.GetValue(PlayingCameraIdProperty);
            set => this.SetValue(PlayingCameraIdProperty, value);
        }

        public string ModeText
        {
            get => (string)this.GetValue(ModeTextProperty);
            set => this.SetValue(ModeTextProperty, value);
        }

        public string EntryText
        {
            get => this.GetValue(EntryTextProperty) as string;
            set => this.SetValue(EntryTextProperty, value);
        }

        public string TimeText
        {
            get => (string)GetValue(TimeTextProperty);
            set => SetValue(TimeTextProperty, value);
        }

        public PresentMode PresentMode
        {
            get => (PresentMode)GetValue(PresentModeProperty);
            set => SetValue(PresentModeProperty, value);
        }

        public System.Drawing.Image SnapshotImage
        {
            get => (System.Drawing.Image)GetValue(SnapshotIageProperty);
            set => SetValue(SnapshotIageProperty, value);
        }


        private static void ETagNumberChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CameraViewer cameraViewer))
                return;
            if (cameraViewer.Camera != null)
                cameraViewer.Camera.ETag = e.NewValue.ToString();
        }

        private static void ValidEtagChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CameraViewer cameraViewer) || !(e.NewValue is bool newValue))
                return;
            if (cameraViewer.Camera != null)
                cameraViewer.Camera.ValidETag = newValue;
        }

        private static void EtagConnectingChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CameraViewer cameraViewer) || !(e.NewValue is bool newValue))
                return;
            if (cameraViewer.Camera != null)
                cameraViewer.Camera.ETagConnecting = newValue;
        }

        private static void OpenDoorCommandChanged(
          DependencyObject d,
          DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CameraViewer cameraViewer))
                return;
            if (!(e.NewValue is ICommand command))
                return;
            if (cameraViewer.Camera != null)
                cameraViewer.Camera.OpenDoor = () => command.Execute(null);
        }

        private static void SnapshotImageChanged(
          DependencyObject d,
          DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CameraViewer cameraViewer) || !(e.NewValue is System.Drawing.Image newValue))
                return;
            if (cameraViewer.Camera != null)
                cameraViewer.Camera.SnapshotImage = newValue;
        }

        private static void PresentModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CameraViewer cameraViewer))
                return;
            var newValue = (PresentMode)e.NewValue;
            if (cameraViewer.Camera != null)
                cameraViewer.Camera.PresentMode = newValue;
        }

        private static void PlayingCameraIdChanged(
          DependencyObject d,
          DependencyPropertyChangedEventArgs e)
        {

            if (!(d is CameraViewer cameraViewer))
                return;
            IntPtr newValue = (IntPtr)e.NewValue;
            if (newValue == IntPtr.Zero)
                return;

            var vm = cameraViewer.DataContext as CameraViewerViewModel;
            CameraServiceFactory.Create(vm.ChannelViewModel.CameraType).StartVideo(newValue, cameraViewer.PanelHandle);
        }

        private static void ModeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CameraViewer cameraViewer))
                return;
            string newValue = (string)e.NewValue;
            if (cameraViewer.Camera != null)
                cameraViewer.Camera.ModeText = newValue;
        }

        private static void EntryTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CameraViewer cameraViewer))
                return;
            string newValue = (string)e.NewValue;
            if (cameraViewer.Camera != null)
                cameraViewer.Camera.EntryText = newValue;
        }

        private static void TimeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CameraViewer cameraViewer))
                return;
            string newValue = (string)e.NewValue;
            if (cameraViewer.Camera != null)
                cameraViewer.Camera.TimeText = newValue;
        }

        private IntPtr PanelHandle => Camera.panelHandle;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var element = new WindowsFormsHost
            {
                Background = new SolidColorBrush(Colors.Gray),
                FontSize = 12.0
            };
            if (Camera != null)
            {
                Camera.Dock = DockStyle.Fill;
                element.Child = Camera;
                grid.Children.Add(element);
            }
        }

        private void Camera_OpenDoor(object sender, EventArgs e)
        {

        }
    }
}
