using System;
using System.Threading;
using System.Threading.Tasks;

namespace Illumine.LPR
{
    public class ChannelViewModel : BaseViewModel
    {
        private TaskCompletionSource<object> _tcs;
        private TaskCompletionSource<object> _cancel;

        public async Task<bool> WaitActual()
        {
            _cancel = new TaskCompletionSource<object>();
            _tcs = new TaskCompletionSource<object>();

            Task completeTask = await Task.WhenAny(_tcs.Task, _cancel.Task);
            if (completeTask == _tcs.Task)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Abandon()
        {
            if (App.Initialized)
                _cancel?.TrySetResult(null);
        }

        public void Actual()
        {
            if (App.Initialized)
                _tcs?.TrySetResult(null);
        }



        public bool Enabled { get; set; } = true;

        public bool ExtOutput { get; set; }

        public string ExtSubFolder { get; set; }

        public int Id { get; set; }

        public string SensorIp { get; set; }

        public string Url { get; set; }

        public string Led1Ip { get; set; }

        public int Led1Port { get; set; }

        public string Led2Ip { get; set; }

        public int Led2Port { get; set; }

        public string Line1Normal { get; set; } = "";

        public string Line2Normal { get; set; } = "";

        public string Line1NoVip { get; set; } = "";

        public string Line2NoVip { get; set; } = "";


        public string Line1Fail { get; set; } = "";

        public string Line2Fail { get; set; } = "";

        public string Line1Active { get; set; } = "";

        public string Line2Active { get; set; } = "";


        public EntryMode EntryMode { get; set; }

        public VehicleMode VehicleMode { get; set; }

        public CameraType CameraType { get; set; }

        public CameraType BackCameraType { get; set; }

        public string EtagReaderIp { get; set; }

        public bool ValidEtag => EtagReaderIp != "";

        public bool EtagReaderConnecting { get; set; }

        public string EtagNumber { get; set; }

        public string BackUrl { get; set; }

        public PresentMode PresentMode { get; set; } = PresentMode.OnlySnapshot;

        public int InitialTimeout { get; set; } = 3000;

        //public VipData CurrentVip { get; set; }

        public string EntryName => ChannelService.GetEntryName(this.Id);
    }
}
