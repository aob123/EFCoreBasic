using System;
using EFCOREBASIC.Models;

using CsvHelper;
using System.Globalization;

string inputFile = "./TempFuktData.csv";
List<WeatherModel> outputRecords = new();

using var reader = new StreamReader(inputFile);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

var records = csv.GetRecords<WeatherModel>();
int counter = 0;

foreach (var record in records)
{

    record.Temp = record.Temp
    .Replace('\u2013', '-')  // EN DASH
    .Replace('\u2014', '-')  // EM DASH
    .Replace('\u2212', '-')  // MINUS SIGN
    .Trim(); 


        // Define NumberFormatInfo
        var format = new NumberFormatInfo
        {
            NegativeSign = "-", // Define standard negative sign
            NumberDecimalSeparator = "."
        };

        // Parse the string
        bool success = decimal.TryParse(record.Temp, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, format, out decimal result);
        counter++;
        if (success)
        {
            // Console.WriteLine($"Parsed successfully: {result}");
            // System.Console.WriteLine($"#{counter} || {record.Datum} | {record.Plats} | {result} | {record.Luftfuktighet}");
            insertData(record.Datum, record.Plats, result, record.Luftfuktighet);
        }
        else
        {
            Console.WriteLine("Failed to parse the number.");
        }

 
}


// readProduct();

// static void readProduct()
// {
//     using var db = new EFContext();
//     List<WeatherData> datas = db.WeatherData.ToList();
//     foreach (WeatherData d in datas)
//     {
//         Console.WriteLine($"ID: {d.Datum} | Name: {d.Plats}");
//     }
//     return;
// }
static void insertData(DateTime date, string location, decimal temp, int hum)
{
    using var db = new EFContext();
    WeatherData data = new WeatherData();
    data.Datum = date;
    data.Plats = location;
    data.Temp = temp;
    data.Luftfuktighet = hum;

    if (!db.WeatherData.Contains(data))
    {
        db.Add(data);
        db.SaveChanges();
    }
    else 
    {
        System.Console.WriteLine("Record already exists");
    }

}
