using Illumine.LPR.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Illumine.LPR
{
    public class LPRSettingService
    {
        private const string PATH = "./Setting.xml";

        public static void Init()
        {
            XDocument xdocument;
            using (StreamReader streamReader = new StreamReader("./Setting.xml", Container.Get<Encoding>()))
                xdocument = XDocument.Load(streamReader);
            Container.Put(xdocument.Descendants("Channel").Select(x => ItemConverter<ChannelDataModel>.GetData(x)).ToList());
            LPRSetting data = ItemConverter<LPRSetting>.GetData(xdocument.Element("Illumine.LPR").Element("LPR"));
            Container.Put(data);

            if (!data.IsVipEnabed)
                return;
            Container.Put(ItemConverter<RelaySetting>.GetData(xdocument.Element("Illumine.LPR").Element("Relay")));
        }

        public static void Save()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement element1 = doc.CreateElement("Illumine.LPR");
            doc.AppendChild((XmlNode)element1);
            LPRSetting data1 = Container.Get<LPRSetting>();
            XmlNode dataNode1 = doc.CreateDataNode(data1);
            element1.AppendChild(dataNode1);
            if (data1.IsVipEnabed)
            {
                RelaySetting data2 = Container.Get<RelaySetting>();
                XmlNode dataNode2 = doc.CreateDataNode(data2);
                element1.AppendChild(dataNode2);
            }
            List<ChannelDataModel> channelDataModelList = Container.Get<List<ChannelDataModel>>();
            XmlElement element2 = doc.CreateElement("Channels");
            element1.AppendChild(element2);
            foreach (ChannelDataModel data3 in channelDataModelList)
            {
                XmlNode dataNode3 = doc.CreateDataNode(data3);
                element2.AppendChild(dataNode3);
            }
            using (StreamWriter writer = new StreamWriter("./Setting.xml", false, Container.Get<Encoding>()))
                doc.Save(writer);
        }
    }
}
