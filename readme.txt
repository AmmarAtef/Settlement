DAM Settlement Point Data Processor

- Required Tasks
1. Extract the DAM Settlement Point Prices data from the given URL.
2. Clean and structure the data, ensuring it is in a tabular format suitable for analysis. Handle any missing values, data types, and other inconsistencies.
4. Design a database table to store the data.

- Solution Guide

Project Setup and Migration
1. Open the command prompt in your project directory (.csproj location)
2. Generate the migration file:

dotnet ef migrations add InitialMigration --project DAMSettlementPoint

3. Apply database update:

dotnet ef database update --project DAMSettlementPoint


- Project Components
1. **Data Extraction**
   - XML file reading and parsing
   - Settlement point data extraction

2. **Data Cleaning Service**
   - Handles missing values
   - Validates data types
   - Ensures data consistency
   - Converts to tabular format

4. **Database Implementation**
   - EF Core Code First approach
   - SQL Server database
   - Proper model configurations
   - Data storage operations

- Code Location
- Main processing logic is in Program.cs
- Data cleaning service in SettlementDataCleaner.cs
- Database context in SettlementContext.cs


** Please now that you will find the xmlFile in  XmlFile folder **




