using System.Collections.ObjectModel;

namespace Illumine.LPR
{
  public class EditRelayService
  {
    public static ObservableCollection<RelaySettingCheckBoxItemViewModel> GetCheckBoxItem(
      bool[] ary)
    {
      ObservableCollection<RelaySettingCheckBoxItemViewModel> checkBoxItem = new ObservableCollection<RelaySettingCheckBoxItemViewModel>();
      for (int index = 0; index < ary.Length; ++index)
        checkBoxItem.Add(new RelaySettingCheckBoxItemViewModel()
        {
          Index = index,
          Label = (index + 1).ToString(),
          IsChecked = ary[index]
        });
      return checkBoxItem;
    }
  }
}
