using System.Xml.Linq;

namespace DAMSettlementPoint.Entities.XmlEntities
{
    public class DAMSettlementPointPrice
    {
        public string DeliveryDate { get; set; }
        public string HourEnding { get; set; }
        public string SettlementPoint { get; set; }
        public decimal SettlementPointPrice { get; set; }
        public string DSTFlag { get; set; }

        public static DAMSettlementPointPrice FromXElement(XElement element)
        {
            XNamespace ns = "http://www.ercot.com/schema/2009-01/nodal/cdr";
            return new DAMSettlementPointPrice
            {
                DeliveryDate = element.Element(ns + "DeliveryDate")?.Value,
                HourEnding = element.Element(ns + "HourEnding")?.Value,
                SettlementPoint = element.Element(ns + "SettlementPoint")?.Value,
                SettlementPointPrice = decimal.Parse(element.Element(ns + "SettlementPointPrice")?.Value ?? "0"),
                DSTFlag = element.Element(ns + "DSTFlag")?.Value
            };
        }
    }
}
