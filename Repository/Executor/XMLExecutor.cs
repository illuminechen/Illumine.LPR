using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Illumine.LPR.Repository
{
    public class XMLExecutor : BaseExecutor
    {
        public string FilePath { get; }

        public XMLExecutor(string path)
          : base(path)
        {
            this.FilePath = path;
        }

        public override void Create()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.AppendChild(xmlDocument.CreateElement("LPR"));
            xmlDocument.Save(this.FileName);
        }

        public override void CreatePages(List<PageBase> pages)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(this.FileName);
            XmlNode root = doc.SelectSingleNode("/LPR");
            pages.ForEach(x => root.AppendChild(doc.CreateElement(x.PageName + "s")));
            doc.Save(this.FileName);
        }

        public override void Delete<Data>(PageBase page, Data data)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(this.FileName);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("/LPR/" + page.PageName + "s");
            XmlNode oldChild = xmlDocument.SelectSingleNode(string.Format("/LPR/{0}s/{1}[Id={2}]", page.PageName, page.PageName, data.Id));
            if (oldChild != null && xmlNode != null)
                xmlNode.RemoveChild(oldChild);
            xmlDocument.Save(this.FileName);
        }

        public override void Insert<Data>(PageBase page, Data data)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(this.FileName);
            string xpath = "/LPR/" + page.PageName + "s";
            XmlNode xmlNode = xmlDocument.SelectSingleNode(xpath);
            XmlElement element1 = xmlDocument.CreateElement(page.PageName);
            Dictionary<string, string> dict = ItemConverter<Data>.GetDict(data);
            foreach (string fieldName in page.FieldNames)
            {
                XmlElement element2 = xmlDocument.CreateElement(fieldName);
                element2.InnerText = dict[fieldName];
                element1.AppendChild(element2);
            }
            xmlNode.AppendChild(element1);
            xmlDocument.Save(this.FileName);
        }

        public override void Update<Data>(PageBase page, Data data)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(this.FileName);
            XmlNode xmlNode = xmlDocument.SelectSingleNode(string.Format("/LPR/{0}s/{1}[Id={2}]", page.PageName, page.PageName, data.Id));
            Dictionary<string, string> dict = ItemConverter<Data>.GetDict(data);
            foreach (string fieldName in page.FieldNames)
                xmlNode.SelectSingleNode(fieldName).InnerText = dict[fieldName];
            xmlDocument.Save(this.FileName);
        }

        public override List<Data> Read<Data>(PageBase page, int limit = -1, SortOrder order = SortOrder.Unspecified)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(this.FileName);
            return xmlDocument.SelectNodes("/LPR/" + page.PageName + "s/" + page.PageName).Cast<XmlNode>().Select(x => ItemConverter<Data>.GetData(XElement.Parse(x.OuterXml))).ToList<Data>();
        }

        public override void ResetId<Data>(PageBase page, Data data, int newId)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(this.FileName);
            xmlDocument.SelectSingleNode(string.Format("/LPR/{0}s/{1}[Id={2}]", page.PageName, page.PageName, data.Id)).SelectSingleNode("Id").InnerText = newId.ToString();
            xmlDocument.Save(this.FileName);
        }

        public override bool TryGetFieldNames(PageBase page, out string[] FieldNames)
        {
            FieldNames = new string[] { };
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(this.FileName);
                var dataNode = xmlDocument.SelectSingleNode("/LPR/" + page.PageName + "s/" + page.PageName);
                if (dataNode == null)
                    return false;
                FieldNames = dataNode.ChildNodes.Cast<XmlNode>().Select(x => x.Name).ToArray();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

    }
}
