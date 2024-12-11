using DAMSettlementPoint.Data_Identity;
using DAMSettlementPoint.Entities.Models;
using DAMSettlementPoint.Entities.XmlEntities;
using DAMSettlementPoint.Services;
using System.Data;

SettlementDataProcessor settlementDataProcessor = new SettlementDataProcessor(
                @"C:\Users\ammar\source\repos\DAMSettlementPoint\DAMSettlementPoint\XmlFile\DAMSettlementPoint.xml"
            );

var prices = settlementDataProcessor.LoadFromFile();
Console.WriteLine($"Successfully loaded {prices.Count} records");



Console.WriteLine("Hello, World!");





//2.Clean and structure the data. 4. store the data  in my database.  

try
{
    var cleaner = new SettlementDataCleaner(@"C:\Users\ammar\source\repos\DAMSettlementPoint\DAMSettlementPoint\XmlFile\DAMSettlementPoint.xml");
    Console.WriteLine("Processing settlement data...");
    var (cleanData, validationErrors) = cleaner.ProcessData();

    if (validationErrors.Count > 0)
    {
        Console.WriteLine("\nValidation Errors Found:");
        foreach (var error in validationErrors)
        {
            Console.WriteLine($"Row {error.Row}: {error.ErrorMessage} (Value: {error.OriginalValue})");
        }
    }

    Console.WriteLine($"\nProcessed {cleanData.Rows.Count} valid records:");
    using (var context = new SettlementContext())
    {
        foreach (DataRow row in cleanData.Rows)
        {
            var settlement = new SettlementPoint
            {
                DeliveryDate = (DateTime)row["DeliveryDate"],
                HourEnding = (TimeSpan)row["HourEnding"],
                SettlementPointName = row["SettlementPoint"].ToString(),
                SettlementPointPrice = (decimal)row["SettlementPointPrice"],
                DSTFlag = (bool)row["DSTFlag"],
                CreatedDate = DateTime.Now
            };

            context.SettlementPoints.Add(settlement);

            Console.WriteLine(
                $"Date: {row["DeliveryDate"],-12} " +
                $"Hour: {row["HourEnding"],-8} " +
                $"Point: {row["SettlementPoint"],-15} " +
                $"Price: ${row["SettlementPointPrice"],-8} " +
                $"DST: {row["DSTFlag"]}"
            );
        }

        int recordsSaved = context.SaveChanges();
        Console.WriteLine($"\nSuccessfully saved {recordsSaved} records to database.");
    }
    Console.WriteLine("\nPress any key to exit...");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

