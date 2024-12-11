using System.Data;
using System.Globalization;
using System.Xml.Linq;

namespace DAMSettlementPoint.Services
{
    public class SettlementDataCleaner
    {
        private readonly string filePath;
        private readonly CultureInfo cultureInfo = new CultureInfo("en-US");
        private readonly XNamespace ns = "http://www.ercot.com/schema/2009-01/nodal/cdr";

        public SettlementDataCleaner(string filePath)
        {
            this.filePath = filePath;
        }

        public DataTable CreateCleanDataTable()
        {
            var dt = new DataTable("SettlementData");
            dt.Columns.Add("DeliveryDate", typeof(DateTime));
            dt.Columns.Add("HourEnding", typeof(TimeSpan));
            dt.Columns.Add("SettlementPoint", typeof(string));
            dt.Columns.Add("SettlementPointPrice", typeof(decimal));
            dt.Columns.Add("DSTFlag", typeof(bool));
            return dt;
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }
            public int Row { get; set; }
            public string OriginalValue { get; set; }
        }

        public (DataTable CleanData, List<ValidationResult> ValidationErrors) ProcessData()
        {
            var cleanData = CreateCleanDataTable();
            var validationErrors = new List<ValidationResult>();
            var rowCount = 0;

            try
            {
                var doc = XDocument.Load(filePath);
                foreach (var element in doc.Descendants(ns + "DAMSettlementPointPrice"))
                {
                    rowCount++;
                    var row = cleanData.NewRow();

                    ValidateAndSetDeliveryDate(element, row, rowCount, validationErrors);
                    ValidateAndSetHourEnding(element, row, rowCount, validationErrors);
                    ValidateAndSetSettlementPoint(element, row, rowCount, validationErrors);
                    ValidateAndSetPrice(element, row, rowCount, validationErrors);
                    ValidateAndSetDSTFlag(element, row, rowCount, validationErrors);

                    if (!validationErrors.Any(e => e.Row == rowCount))
                    {
                        cleanData.Rows.Add(row);
                    }
                }

                AddComputedColumns(cleanData);

                return (cleanData, validationErrors);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing settlement data: {ex.Message}", ex);
            }
        }

        private void ValidateAndSetDeliveryDate(XElement element, DataRow row, int rowCount, List<ValidationResult> errors)
        {
            var dateStr = element.Element(ns + "DeliveryDate")?.Value;
            if (DateTime.TryParse(dateStr, cultureInfo, DateTimeStyles.None, out DateTime date))
            {
                row["DeliveryDate"] = date;
            }
            else
            {
                errors.Add(new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid delivery date format",
                    Row = rowCount,
                    OriginalValue = dateStr
                });
            }
        }

        private void ValidateAndSetHourEnding(XElement element, DataRow row, int rowCount, List<ValidationResult> errors)
        {
            var hourStr = element.Element(ns + "HourEnding")?.Value;
            if (TimeSpan.TryParse(hourStr?.Replace("24:00", "00:00"), out TimeSpan time))
            {
                row["HourEnding"] = time;
            }
            else
            {
                errors.Add(new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid hour ending format",
                    Row = rowCount,
                    OriginalValue = hourStr
                });
            }
        }

        private void ValidateAndSetSettlementPoint(XElement element, DataRow row, int rowCount, List<ValidationResult> errors)
        {
            var point = element.Element(ns + "SettlementPoint")?.Value;
            if (!string.IsNullOrWhiteSpace(point))
            {
                row["SettlementPoint"] = point.Trim();
            }
            else
            {
                errors.Add(new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Missing or invalid settlement point",
                    Row = rowCount,
                    OriginalValue = point
                });
            }
        }

        private void ValidateAndSetPrice(XElement element, DataRow row, int rowCount, List<ValidationResult> errors)
        {
            var priceStr = element.Element(ns + "SettlementPointPrice")?.Value;
            if (decimal.TryParse(priceStr, NumberStyles.Any, cultureInfo, out decimal price))
            {
                row["SettlementPointPrice"] = Math.Round(price, 2);
            }
            else
            {
                errors.Add(new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid price format",
                    Row = rowCount,
                    OriginalValue = priceStr
                });
            }
        }

        private void ValidateAndSetDSTFlag(XElement element, DataRow row, int rowCount, List<ValidationResult> errors)
        {
            var dstFlag = element.Element(ns + "DSTFlag")?.Value?.Trim().ToUpper();
            if (dstFlag == "Y" || dstFlag == "N")
            {
                row["DSTFlag"] = dstFlag == "Y";
            }
            else
            {
                errors.Add(new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid DST flag value",
                    Row = rowCount,
                    OriginalValue = dstFlag
                });
            }
        }

        private void AddComputedColumns(DataTable dt)
        {
            dt.Columns.Add("DayOfWeek", typeof(string));
            foreach (DataRow row in dt.Rows)
            {
                row["DayOfWeek"] = ((DateTime)row["DeliveryDate"]).DayOfWeek.ToString();
            }

            dt.Columns.Add("PriceVariationFromAvg", typeof(decimal));
            var groups = dt.AsEnumerable()
                .GroupBy(r => r.Field<DateTime>("DeliveryDate"));

            foreach (var group in groups)
            {
                var avgPrice = group.Average(r => r.Field<decimal>("SettlementPointPrice"));
                foreach (var row in group)
                {
                    row["PriceVariationFromAvg"] = Math.Round(
                        row.Field<decimal>("SettlementPointPrice") - avgPrice, 2
                    );
                }
            }
        }

        public DataTable GetPivotedData()
        {
            var (cleanData, _) = ProcessData();
            var pivotTable = new DataTable("PivotedSettlementData");

            pivotTable.Columns.Add("DeliveryDate", typeof(DateTime));

            for (int i = 1; i <= 24; i++)
            {
                pivotTable.Columns.Add($"Hour{i:D2}", typeof(decimal));
            }

            var groupedData = cleanData.AsEnumerable()
                .GroupBy(r => new
                {
                    Date = r.Field<DateTime>("DeliveryDate"),
                    Point = r.Field<string>("SettlementPoint")
                });

            foreach (var group in groupedData)
            {
                var row = pivotTable.NewRow();
                row["DeliveryDate"] = group.Key.Date;

                foreach (var record in group)
                {
                    var hour = record.Field<TimeSpan>("HourEnding").Hours;
                    hour = hour == 0 ? 24 : hour;
                    row[$"Hour{hour:D2}"] = record.Field<decimal>("SettlementPointPrice");
                }

                pivotTable.Rows.Add(row);
            }

            return pivotTable;
        }
    }
}
