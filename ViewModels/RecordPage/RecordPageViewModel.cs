using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class RecordPageViewModel : BaseViewModel
    {
        public int FilterMode { get; set; }

        public RecordViewModel SelectedRecord { get; set; }

        public PlateSnapshotViewModel SelectedSnapshot => this.SelectedRecord != null ? SnapshotService.GetViewModel(SelectedRecord) : (PlateSnapshotViewModel)null;

        public ObservableCollection<RecordViewModel> RecordList => this.getRecordList();

        public DateTime BeginDate { get; set; } = DateTime.Now.Date;

        public DateTime FinishDate { get; set; } = DateTime.Now.Date;

        public string PlateNumber { get; set; } = "";

        public bool UseDate { get; set; }

        public bool UsePlate { get; set; }

        public int RecordCount => this.RecordList.Count;

        public ICommand ExportCommand { get; set; }

        private ObservableCollection<RecordViewModel> getRecordList()
        {
            Dictionary<int, EntryMode> dict = new Dictionary<int, EntryMode>()
            {
                {1,EntryMode.In},
                {2,EntryMode.Out}
            };
            List<RecordViewModel> list = RecordService.GetViewModels().ToList<RecordViewModel>();
            DateTime begin = this.BeginDate;
            DateTime finish = this.FinishDate;
            string plateNumber = this.PlateNumber;
            if (this.FilterMode != 0)
                list = list.FindAll((Predicate<RecordViewModel>)(x => ChannelService.GetEntryMode(x.ChannelId) == dict[this.FilterMode]));
            if (this.UseDate)
                list = list.FindAll((Predicate<RecordViewModel>)(x => x.TimeStamp >= begin && x.TimeStamp < finish.AddDays(1.0)));
            if (this.UsePlate)
                list = list.FindAll((Predicate<RecordViewModel>)(x => x.CarPlateViewModel.PlateNumber.Contains(this.PlateNumber)));
            return new ObservableCollection<RecordViewModel>(list);
        }

        public RecordPageViewModel() => this.ExportCommand = (ICommand)new RelayCommand(new Action(this.Export));

        private void Export()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Application.StartupPath;
            saveFileDialog1.FileName = DateTime.Now.ToString("yyyyMMdd") + "紀錄";
            saveFileDialog1.Title = "匯出紀錄";
            saveFileDialog1.Filter = "Excel活頁簿|*.xlsx|CSV(逗號分隔)|*.csv|文字檔(Tab 字元分隔)|*.txt|Sqlite資料庫|*.db";
            SaveFileDialog saveFileDialog2 = saveFileDialog1;
            if (saveFileDialog2.ShowDialog() != DialogResult.OK)
                return;
            File.Delete(saveFileDialog2.FileName);
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            RepositoryService.OutputMsg(saveFileDialog2.FileName, RepoTypeHelper.GetRepoType(saveFileDialog2.FileName));
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            int num = (int)MessageBox.Show("匯出完成");
        }
    }
}
