using Illumine.LPR.Repository;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Illumine.LPR
{
    public static class XmlDocumentExtension
    {
        public static XmlNode CreateDataNode<T>(this XmlDocument doc, T data, bool useInnerText = true) where T : class, new()
        {
            string name = typeof(T).GetCustomAttributes(typeof(DisplayNameAttribute), true).OfType<DisplayNameAttribute>().FirstOrDefault<DisplayNameAttribute>()?.DisplayName ?? typeof(T).Name;
            XmlElement element1 = doc.CreateElement(name);
            foreach (KeyValuePair<string, string> keyValuePair in ItemConverter<T>.GetDict((object)data))
            {
                if (useInnerText)
                {
                    XmlElement element2 = doc.CreateElement(keyValuePair.Key);
                    element2.InnerText = keyValuePair.Value;
                    element1.AppendChild((XmlNode)element2);
                }
                else
                    element1.SetAttribute(keyValuePair.Key, keyValuePair.Value);
            }
            return (XmlNode)element1;
        }
    }
}
