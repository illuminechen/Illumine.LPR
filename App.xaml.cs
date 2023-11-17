using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Threading;

namespace Illumine.LPR
{
    public partial class App : Application
    {
        public static bool Initialized = false;

        private const string MutexName = "LPR_Illumine";
        private Mutex _mutex;
        private bool _isFirstInstance;
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            var list = Container.Get<List<ChannelDataModel>>();

            list?.ForEach(x =>
            {
                Container.Get<string, ModbusTCPService>(x.SensorIp)?.StopMonitor(x.EntryMode);
                Container.Get<string, ModbusTCPService>(x.SensorIp)?.Disconnect();
            });

            LEDService.StopServer();
            // 執行完畢後釋放 Mutex
            if (_mutex != null)
            {
                if (_mutex.WaitOne(0))
                {
                    _mutex.ReleaseMutex();
                }
                _mutex.Dispose();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                _mutex = new Mutex(true, MutexName, out _isFirstInstance);
                if (!_isFirstInstance)
                {
                    // 如果不是第一個執行實例，則表示已經有其他實例在運行，可以採取適當的處理方式，例如結束應用程式或顯示訊息。
                    System.Windows.Forms.MessageBox.Show("Another instance is already running.");
                    Shutdown();
                }
                else
                /*
                (bool success, string messages) = LicenseService.CheckLicenseFile(Environment.CurrentDirectory + "/License.dat");
                if (!success)
                {
                    LicenseWindow licenseWindow = new LicenseWindow();
                    licenseWindow.DataContext = new LicenseWindowViewModel()
                    {
                        Reason = messages
                    };
                    Current.MainWindow = licenseWindow;
                    Current.MainWindow.Show();
                }
                else*/
                {

                    ApplicationSetup();
                    Container.Put(System.Text.Encoding.GetEncoding(950));
                    Container.Put(new ProgressDialog() { DataContext = new ProgressDialogViewModel() });
                    //Task.Run(() =>
                    //{
                    LPRSettingService.Init();
                    LEDService.StartServer();
                    List<CameraType> cameraTypes = Container.Get<List<ChannelDataModel>>().Select(o => o.CameraType).ToList();
                    List<CameraType> cameraTypes2 = Container.Get<List<ChannelDataModel>>().Where(o => o.BackCameraIp != "").Select(o => o.BackCameraType).ToList();

                    cameraTypes.Concat(cameraTypes2).Distinct().ToList().ForEach(x => CameraServiceFactory.Create(x).Init());

                    RepositoryService.UpdateSchema();
                    DataInitializer.Setup();

                    if (Container.Get<LPRSetting>().ETagMode != ETagMode.No && !EtagService.Init())
                        System.Windows.Forms.MessageBox.Show("ETagServer未正確開啟");

                    //Task.Delay(5000);
                    //}).ContinueWith(t =>
                    //{
                    //Current.Dispatcher.Invoke(() =>
                    //{
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.DataContext = new MainWindowViewModel();
                    Current.MainWindow = mainWindow;
                    Current.MainWindow.Show();
                    Container.Get<ProgressDialog>().Hide();
                    //    });
                    //});
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                LogHelper.Log(ex);
            }
            finally
            {
            }
        }

        private void ApplicationSetup() => TypeDescriptor.AddAttributes(typeof(bool[]), new Attribute[1]
        {
           new TypeConverterAttribute(typeof (BoolArrayConverter))
        });
    }
}