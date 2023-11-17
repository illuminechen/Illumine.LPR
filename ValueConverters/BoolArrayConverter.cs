using System;
using System.ComponentModel;
using System.Globalization;

namespace Illumine.LPR
{
    public class BoolArrayConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object ConvertFrom(
          ITypeDescriptorContext context,
          CultureInfo culture,
          object value)
        {
            if (!(value is string str))
                return base.ConvertFrom(context, culture, value);
            bool[] flagArray = new bool[str.Length];
            for (int index = 0; index < str.Length; ++index)
                flagArray[index] = str[index] != '0';
            return (object)flagArray;
        }

        public override object ConvertTo(
          ITypeDescriptorContext context,
          CultureInfo culture,
          object value,
          Type destinationType)
        {
            if (!(destinationType == typeof(string)) || !(value is bool[] flagArray))
                return base.ConvertTo(context, culture, value, destinationType);
            string str = "";
            for (int index = 0; index < flagArray.Length; ++index)
            {
                bool flag = flagArray[index];
                str += flag ? "1" : "0";
            }
            return (object)str;
        }
    }
}
