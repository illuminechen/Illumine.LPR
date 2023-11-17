using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Illumine.LPR
{
    public class GroupViewModel : BaseViewModel
    {
        public GroupData groupData => GroupDataService.GetData(this.Id);

        public int Id { get; set; }

        [RegularExpression("^(\\w)*$", ErrorMessage = "不可包含特殊符號")]
        public string GroupName { get; set; }

        public int TotalCount { get; set; }

        public int CurrentCount { get; set; }

        public int DisplayCurrentCount
        {
            get => CurrentCount;
            set
            {
                CurrentCount = Math.Max(0, Math.Min(value, TotalCount));
            }
        }

        public GroupViewModel(GroupData data)
        {
            this.Id = data.Id;
            this.GroupName = data.GroupName;
            this.TotalCount = data.TotalCount;
            this.CurrentCount = data.CurrentCount;
            this.PropertyChanged += GroupViewModel_PropertyChanged;
        }

        private void GroupViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Id")
                return;
            PropertyInfo property1 = typeof(GroupViewModel).GetProperty(e.PropertyName);
            PropertyInfo property2 = typeof(GroupData).GetProperty(e.PropertyName);
            if (property2 == null || property1 == null || this.groupData == null)
                return;

            object upper = property1.GetValue(this);

            if (GetType().GetProperty(e.PropertyName).GetCustomAttributes().OfType<UpperCaseAttribute>().ToList().Count != 0 && upper is string str && str.ToUpper() != str)
            {
                property1.SetValue(this, str.ToUpper());
                upper = str.ToUpper();
            }

            if (this[e.PropertyName] != "")
                return;
            property2.SetValue(groupData, upper);
            RepositoryService.Update(((GroupViewModel)sender).groupData);
        }

        public GroupViewModel()
        {
            this.PropertyChanged += GroupViewModel_PropertyChanged;
        }

    }
}