using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Illumine.LPR.Repository
{
    public static class ItemConverter
    {
        public static object GetData(Type type, DataRow dr)
        {
            Dictionary<string, string> dictionary = dr.Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToDictionary(c => c, c => dr[c].ToString());
            return GetData(type, dictionary);
        }

        public static object GetData(Type type, XElement xElement)
        {
            xElement.Elements();
            Dictionary<string, string> dictionary = xElement.Elements().ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
            return GetData(type, dictionary);
        }

        public static object GetData(Type type, string title, string line)
        {
            string[] titles = title.Split(',');
            string[] dataAry = line.Split(',');
            Dictionary<string, string> dictionary = Enumerable.Range(0, titles.Length).ToDictionary(i => titles[i].Trim('"'), i => dataAry[i].Trim('"'));
            return GetData(type, dictionary);
        }

        public static object GetData(Type type, Dictionary<string, string> dict)
        {
            object instance = Activator.CreateInstance(type);
            foreach (KeyValuePair<string, string> keyValuePair in dict)
            {
                PropertyInfo property = type.GetProperty(keyValuePair.Key);
                if (property.CanWrite)
                    property.SetValue(instance, TypeDescriptor.GetConverter(property.PropertyType).ConvertFromString(keyValuePair.Value));
            }
            return instance;
        }

        public static Dictionary<string, string> GetDict(Type type, object data)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    object obj = property.GetValue(data);
                    string str = TypeDescriptor.GetConverter(property.PropertyType).ConvertToString(obj);
                    dict.Add(property.Name, str);
                }
            }
            return dict;
        }
    }

    public static class ItemConverter<DtoData> where DtoData : class, new()
    {
        public static DtoData GetData(DataRow dr) => GetData(dr.Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToDictionary(c => c, c => dr[c].ToString()));

        public static DtoData GetData(XElement xElement)
        {
            xElement.Elements();
            return GetData(xElement.Elements().ToDictionary(x => x.Name.ToString(), x => x.Value.ToString()));
        }

        public static DtoData GetData(string title, string line, char sep)
        {
            string[] titles = title.Split(sep);
            string[] dataAry = line.Split(sep);
            return GetData(Enumerable.Range(0, titles.Length).ToDictionary(i => titles[i].Trim('"'), i => dataAry[i].Trim('"')));
        }

        public static DtoData GetData(Dictionary<string, string> dict)
        {
            DtoData data = new DtoData();
            foreach (KeyValuePair<string, string> keyValuePair in dict)
            {
                PropertyInfo property = typeof(DtoData).GetProperty(keyValuePair.Key);
                if (property != null && property.CanWrite)
                    property.SetValue(data, TypeDescriptor.GetConverter(property.PropertyType).ConvertFromString(keyValuePair.Value));
            }
            return data;
        }

        public static Dictionary<string, string> GetDict(object data)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (PropertyInfo property in typeof(DtoData).GetProperties())
            {
                object obj = property.GetValue(data);
                string str = TypeDescriptor.GetConverter(property.PropertyType).ConvertToString(obj);
                dict.Add(property.Name, str);
            }
            return dict;
        }
    }

}
