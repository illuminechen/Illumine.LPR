using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Illumine.LPR
{
  public abstract class BaseValueConverter<T> : MarkupExtension, IValueConverter
    where T : class, new()
  {
    private static T Converter;

    public override object ProvideValue(IServiceProvider serviceProvider) => (object) (BaseValueConverter<T>.Converter ?? (BaseValueConverter<T>.Converter = new T()));

    public abstract object Convert(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture);

    public abstract object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture);
  }
}
