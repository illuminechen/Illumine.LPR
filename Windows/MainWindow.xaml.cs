using System;
using System.CodeDom.Compiler;
using System.Windows;
using System.Windows.Markup;

namespace Illumine.LPR
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var dl = System.Windows.Forms.MessageBox.Show("確定要關閉程式？", "關閉程式", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Information, System.Windows.Forms.MessageBoxDefaultButton.Button2);
            if (dl == System.Windows.Forms.DialogResult.OK)
            {
                Container.Get<ProgressDialog>().ForcingClose = true;
                Container.Get<ProgressDialog>().Close();

                if (Container.Get<LPRSetting>().UseRemoteServer && Container.Get<LPRSetting>().HostIp != "")
                    Container.Get<JotangiServerService>().Stop();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
