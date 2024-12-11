
using System.Xml.Linq;

namespace DAMSettlementPoint.Entities.XmlEntities
{


    public class SettlementDataProcessor
    {
        private readonly string filePath;
        private readonly XNamespace ns = "http://www.ercot.com/schema/2009-01/nodal/cdr";

        public SettlementDataProcessor(string filePath)
        {
            this.filePath = filePath;
        }

        public List<DAMSettlementPointPrice> LoadFromFile()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Settlement data file not found at: {filePath}");
                }

                var doc = XDocument.Load(filePath);
                return doc.Descendants(ns + "DAMSettlementPointPrice")
                         .Select(DAMSettlementPointPrice.FromXElement)
                         .ToList();
            }
            catch (Exception ex) when (ex is IOException || ex is System.Xml.XmlException)
            {
                throw new Exception($"Error reading settlement data file: {ex.Message}", ex);
            }
        }

        public void SaveToFile(IEnumerable<DAMSettlementPointPrice> prices)
        {
            try
            {
                var doc = new XDocument(
                    new XElement(ns + "DAMSettlementPointPrices",
                        new XAttribute("xmlns", ns.NamespaceName),
                        new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                        prices.Select(p => ToXElementWithNamespace(p))
                    )
                );
                doc.Save(filePath);
            }
            catch (Exception ex) when (ex is IOException || ex is System.Xml.XmlException)
            {
                throw new Exception($"Error saving settlement data: {ex.Message}", ex);
            }
        }

        private XElement ToXElementWithNamespace(DAMSettlementPointPrice price)
        {
            return new XElement(ns + "DAMSettlementPointPrice",
                new XElement(ns + "DeliveryDate", price.DeliveryDate),
                new XElement(ns + "HourEnding", price.HourEnding),
                new XElement(ns + "SettlementPoint", price.SettlementPoint),
                new XElement(ns + "SettlementPointPrice", price.SettlementPointPrice),
                new XElement(ns + "DSTFlag", price.DSTFlag)
            );
        }

        public List<DAMSettlementPointPrice> GetPricesForDate(string date)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                return doc.Descendants(ns + "DAMSettlementPointPrice")
                         .Where(e => e.Element(ns + "DeliveryDate")?.Value == date)
                         .Select(DAMSettlementPointPrice.FromXElement)
                         .ToList();
            }
            catch (Exception ex) when (ex is IOException || ex is System.Xml.XmlException)
            {
                throw new Exception($"Error querying settlement data: {ex.Message}", ex);
            }
        }

        public List<string> GetUniqueSettlementPoints()
        {
            try
            {
                var doc = XDocument.Load(filePath);
                return doc.Descendants(ns + "SettlementPoint")
                         .Select(e => e.Value)
                         .Distinct()
                         .ToList();
            }
            catch (Exception ex) when (ex is IOException || ex is System.Xml.XmlException)
            {
                throw new Exception($"Error retrieving settlement points: {ex.Message}", ex);
            }
        }

        public decimal GetAveragePriceForSettlementPoint(string settlementPoint)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                return doc.Descendants(ns + "DAMSettlementPointPrice")
                         .Where(e => e.Element(ns + "SettlementPoint")?.Value == settlementPoint)
                         .Select(e => decimal.Parse(e.Element(ns + "SettlementPointPrice")?.Value ?? "0"))
                         .Average();
            }
            catch (Exception ex) when (ex is IOException || ex is System.Xml.XmlException)
            {
                throw new Exception($"Error calculating average price: {ex.Message}", ex);
            }
        }
    }

}
