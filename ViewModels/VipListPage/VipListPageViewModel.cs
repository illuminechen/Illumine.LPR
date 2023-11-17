using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class VipListPageViewModel : BaseViewModel
    {
        public bool Enabled => Container.Get<LPRSetting>().IsVipEnabed;

        public Action<string> SearchPreAction { get; set; }

        public Action<string> SearchNextAction { get; set; }

        public string SearchText { get; set; } = "";

        public bool ShowSearch { get; set; } = false;

        public bool ShowGroupEditor { get; set; } = false;

        public ObservableCollection<VipViewModel> VipList { get; set; }
        public ObservableCollection<GroupViewModel> GroupList { get; set; }

        public ICommand ExportCommand { get; set; }

        public ICommand ImportCommand { get; set; }

        public ICommand SettingCommand { get; set; }

        public ICommand NextCommand { get; set; }

        public ICommand PreCommand { get; set; }

        public ICommand SearchCommand { get; set; }

        public ICommand GroupCommand { get; set; }

        public ICommand CloseGroupCommand { get; set; }

        public VipListPageViewModel(bool design = false)
        {
            if (design)
            {
                DataInitializer.SetupDesignData();
            }
            AttachData();
            this.ExportCommand = new RelayCommand(new Action(this.Export));
            this.ImportCommand = new RelayCommand(new Action(this.Import));
            this.SettingCommand = new RelayCommand(new Action(this.Setting));
            this.NextCommand = new RelayCommand(new Action(this.Next));
            this.PreCommand = new RelayCommand(new Action(this.Pre));
            this.SearchCommand = new RelayCommand(new Action(this.Search));
            this.GroupCommand = new RelayCommand(new Action(this.Group));
            this.CloseGroupCommand = new RelayCommand(new Action(this.CloseGroup));
        }

        public void AttachData()
        {
            this.GroupList = GroupViewModelService.GetViewModels();
            this.GroupList.CollectionChanged += new NotifyCollectionChangedEventHandler(this.GroupList_CollectionChanged);
            this.OnPropertyChanged("GroupList");

            this.VipList = VipViewModelService.GetViewModels();
            this.VipList.CollectionChanged += new NotifyCollectionChangedEventHandler(this.VipList_CollectionChanged);
            this.OnPropertyChanged("VipList");
        }

        public void CloseGroup()
        {
            ShowGroupEditor = false;
            this.OnPropertyChanged("GroupList");
            foreach (var vip in VipList)
                vip.OnPropertyChanged("Group");
        }

        private void GroupList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IEnumerator enumerator = e.NewItems.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            GroupViewModel current = enumerator.Current as GroupViewModel;
                            current.Id = GroupDataService.PutData(current);
                            current.GroupName = "Group" + current.Id;
                            current.CurrentCount = 0;
                            current.TotalCount = 1;
                        }
                        break;
                    }
                    finally
                    {
                        if (enumerator is IDisposable disposable)
                            disposable.Dispose();
                    }
                case NotifyCollectionChangedAction.Remove:

                    foreach (object oldItem in e.OldItems)
                    {
                        var deletedId = (oldItem as GroupViewModel).Id;
                        VipList.ToList().FindAll(x => x.Group == deletedId).ForEach(x => x.Group = 0);
                        GroupDataService.RemoveAtIndex(deletedId);
                    }
                    GroupDataService.ResetId();
                    GroupViewModelService.ResetId(GroupList, VipList);
                    break;
            }
        }

        private void Group()
        {
            ShowGroupEditor = true;
        }

        private void Search()
        {
            ShowSearch = !ShowSearch;
        }

        private void Next()
        {
            SearchNextAction?.Invoke(SearchText);
        }

        private void Pre()
        {
            SearchPreAction?.Invoke(SearchText);
        }

        private void Setting()
        {
            EditRelayModWindow editRelayModWindow = new EditRelayModWindow();
            editRelayModWindow.DataContext = new EditRelayModViewModel();
            editRelayModWindow.ShowDialog();
        }

        private void Import()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Application.StartupPath;
            openFileDialog1.Title = "匯入名單";
            openFileDialog1.Filter = "Excel活頁簿|*.xlsx|CSV(逗號分隔)|*.csv|文字檔(Tab 字元分隔)|*.txt|Sqlite資料庫|*.db";
            OpenFileDialog openFileDialog2 = openFileDialog1;
            if (openFileDialog2.ShowDialog() != DialogResult.OK)
                return;
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            Task.Run(() =>
            {
                try
                {
                    RepositoryService.InputData(openFileDialog2.FileName, RepoTypeHelper.GetRepoType(openFileDialog2.FileName));
                    Container.Get<ProgressDialog>().Dispatcher.Invoke(() =>
                    {
                        Container.Get<ProgressDialog>().Caption = "匯入完成，請關閉視窗。";
                        Container.Get<ProgressDialog>().End();
                    });
                    Container.Put(RepositoryService.GetGroup());
                    Container.Put(RepositoryService.GetVip());
                    Container.Get<VipListPageViewModel>().AttachData();
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                }
                catch (Exception ex)
                {
                    Container.Get<ProgressDialog>().Dispatcher.Invoke(() =>
                    {
                        Container.Get<ProgressDialog>().Caption = "匯入失敗，請檢查格式。";
                        Container.Get<ProgressDialog>().End();
                    });
                }
            });
        }

        private void Export()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Application.StartupPath;
            saveFileDialog1.FileName = DateTime.Now.ToString("yyyyMMdd") + "Vip名單";
            saveFileDialog1.Title = "匯出名單";
            saveFileDialog1.DefaultExt = ".xlsx";
            saveFileDialog1.Filter = "Excel活頁簿|*.xlsx|CSV(逗號分隔)|*.csv|文字檔(Tab 字元分隔)|*.txt|Sqlite資料庫|*.db";
            SaveFileDialog saveFileDialog2 = saveFileDialog1;
            if (saveFileDialog2.ShowDialog() != DialogResult.OK)
                return;
            Task.Run(() =>
            {
                try
                {
                    File.Delete(saveFileDialog2.FileName);
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                    RepositoryService.OutputData(saveFileDialog2.FileName, RepoTypeHelper.GetRepoType(saveFileDialog2.FileName));
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    Container.Get<ProgressDialog>().Dispatcher.Invoke(() =>
                    {
                        Container.Get<ProgressDialog>().Caption = "匯出完成，請關閉視窗。";
                        Container.Get<ProgressDialog>().End();
                    });
                }
                catch (Exception ex)
                {
                    Container.Get<ProgressDialog>().Dispatcher.Invoke(() =>
                    {
                        Container.Get<ProgressDialog>().Caption = "匯出失敗。";
                        Container.Get<ProgressDialog>().End();
                    });
                }
            });
        }

        private void VipList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IEnumerator enumerator = e.NewItems.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            VipViewModel current = enumerator.Current as VipViewModel;
                            current.Id = VipDataService.PutData(current);
                            current.Name = "Vip";
                            current.ValidPeriods = VipDataService.GetPeriods(current.Id);
                        }
                        break;
                    }
                    finally
                    {
                        if (enumerator is IDisposable disposable)
                            disposable.Dispose();
                    }
                case NotifyCollectionChangedAction.Remove:
                    foreach (object oldItem in e.OldItems)
                        VipDataService.RemoveAtIndex((oldItem as VipViewModel).Id);
                    VipDataService.ResetId();
                    VipViewModelService.ResetId(VipList);

                    break;
            }
        }
    }
}
