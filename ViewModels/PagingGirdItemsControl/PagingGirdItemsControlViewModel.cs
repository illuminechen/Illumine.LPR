using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class PagingGirdItemsControlViewModel : BaseViewModel
    {
        public class PageBundle : BaseViewModel
        {
            public int Index { get; set; }
            public bool Current { get; set; }
        }

        public RelayCommand PreviousPageCommand { get; set; }
        public RelayCommand NextPageCommand { get; set; }
        public RelayCommand<int> GoToPageCommand { get; set; }
        public bool CanNext => CurrentPageIndex < TotalPage;
        public bool CanPrevious => CurrentPageIndex > 1;

        public int NavHeight => NeedPaging ? 35 : 0;

        public bool NeedPaging => TotalPage > 1;

        public List<ChannelViewerViewModel> ChannelViewerViewModelList => ChannelViewerService.GetList();
        public List<int?> ChannelViewerViewModelIdList => Container.Get<List<ChannelDataModel>>().Select(x => (int?)(x.Id)).ToList();

        public int CurrentPageIndex { get; set; } = 1;

        public int TotalPage => (int)Math.Ceiling((decimal)ChannelViewerViewModelList.Count / (UniformGirdColumns * UniformGirdRows));
        public IEnumerable<PageBundle> Pages => Enumerable.Range(1, TotalPage).Select(x => new PageBundle { Index = x, Current = x == CurrentPageIndex });
        public int UniformGirdColumns { get; set; } = 2;

        public int UniformGirdRows { get; set; } = 2;

        public PagingGirdItemsControlViewModel()
        {
            PreviousPageCommand = new RelayCommand(PreviousPage);
            NextPageCommand = new RelayCommand(NextPage);
            GoToPageCommand = new RelayCommand<int>(GoToPage);
        }

        private void GoToPage(int pageIndex)
        {
            CurrentPageIndex = pageIndex;
        }

        private void NextPage()
        {
            CurrentPageIndex = Math.Min(TotalPage, CurrentPageIndex + 1);
        }

        private void PreviousPage()
        {
            CurrentPageIndex = Math.Max(1, CurrentPageIndex - 1);
        }
    }
}
