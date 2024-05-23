using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class ConfigPageViewModel : BaseViewModel
    {
        public ConfigPageViewModel()
        {
            ResetCommand = new RelayCommand(this.Reset);
            SetSpaceCommand = new RelayCommand<int>(SetSpace);
            GetSpaceCommand = new RelayCommand(GetSpace);
            SettingSpaceDisplayNumber = SpaceService.GetSpaceCount();
        }

        public async void SetSpace(int space)
        {
            try
            {
                string url = Container.Get<LPRSetting>().HostIp;
                var res = await HttpClientHelper.PostFormUrlEncodedAsync($"http://{url}:2048/setspace",
                    new Dictionary<string, string>() { { "Authorization", Container.Get<LPRSetting>().JToken } },
                    new Dictionary<string, string>() { { "value", $"{space}" } });

                if (res.Contains("true"))
                {
                    SettingSpaceDisplayNumber = SettingSpaceNumber;
                }
            }
            catch
            {

            }
        }
        public async void GetSpace()
        {
            try
            {
                string url = Container.Get<LPRSetting>().HostIp;
                var res = await HttpClientHelper.GetFormUrlEncodedAsync($"http://{url}:2048/getspace",
                    new Dictionary<string, string>() { { "Authorization", Container.Get<LPRSetting>().JToken } });

                var a = res.IndexOf("value:");
                var b = res.IndexOf("}");
                var num = res.Substring(a + 6, b - a + 6 + 1);
                SettingSpaceDisplayNumber = int.Parse(num);
            }
            catch
            {

            }
        }


        public void Reset()
        {
            try
            {
                var server = Container.Get<JotangiServerService>();
                server.Stop();
                server.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex.ToString());
                MessageBox.Show(ex.Message);
            }
        }

        public ICommand ResetCommand { get; set; }
        public ICommand SetSpaceCommand { get; set; }
        public ICommand GetSpaceCommand { get; set; }

        public int SettingSpaceNumber { get; set; }

        public int SettingSpaceDisplayNumber { get; set; }
    }
}
