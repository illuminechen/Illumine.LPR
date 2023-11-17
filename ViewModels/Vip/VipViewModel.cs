using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Illumine.LPR
{
    public class VipViewModel : BaseViewModel
    {
        public IList<VipViewModel> list => VipViewModelService.GetViewModels();

        public VipData vipData => VipDataService.GetData(this.Id);
        public IList<GroupViewModel> groupList => GroupViewModelService.GetViewModels();

        public int Id { get; set; }

        public int Index { get; set; }

        [RegularExpression("^(\\w)*$", ErrorMessage = "不可包含特殊符號")]
        public string Name { get; set; }

        public int? Group { get; set; }

        [Unique]
        [UpperCase]
        [RegularExpression("[A-Z0-9]{2,4}-{0,1}[A-Z0-9]{2,4}", ErrorMessage = "車牌格式不符")]
        public string PlateNumber { get; set; }

        [Unique]
        [RegularExpression("^(\\d){10}$", ErrorMessage = "Etag辨識格式為10碼數字")]
        public string ETagNumber { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime ExpireTime { get; set; } = DateTime.Now.Date;

        [Required]
        public ObservableCollection<PeriodCheckBoxItemViewModel> ValidPeriods { get; set; }

        private void VipViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Id")
                return;
            PropertyInfo property1 = typeof(VipViewModel).GetProperty(e.PropertyName);
            PropertyInfo property2 = typeof(VipData).GetProperty(e.PropertyName);
            if (property2 == null || property1 == null || this.vipData == null)
                return;

            object newval = property1.GetValue(this);
            object oldval = property2.GetValue(this.vipData);

            if (GetType().GetProperty(e.PropertyName).GetCustomAttributes().OfType<UpperCaseAttribute>().ToList().Count != 0 && newval is string str && str.ToUpper() != str)
            {
                property1.SetValue(this, str.ToUpper());
                newval = str.ToUpper();
            }

            if (this[e.PropertyName] != "")
                return;
            if (newval?.ToString() != oldval?.ToString())
            {
                property2.SetValue(vipData, newval);
                RepositoryService.Update(((VipViewModel)sender).vipData);
            }
        }

        public VipViewModel()
        {
            this.PropertyChanged += VipViewModel_PropertyChanged;
        }

        public VipViewModel(VipData data)
        {
            this.Id = data.Id;
            this.Name = data.Name?.Trim();
            this.PlateNumber = data.PlateNumber?.Trim();
            this.Group = data.Group;
            this.ETagNumber = data.ETagNumber;
            this.Description = data.Description;
            this.ExpireTime = data.ExpireTime;
            this.ValidPeriods = VipDataService.GetPeriods(Id);
            this.PropertyChanged += VipViewModel_PropertyChanged;
        }
    }
}