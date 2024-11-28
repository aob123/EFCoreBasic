using System;
using EFCOREBASIC.Models;

using CsvHelper;
using System.Globalization;
using System.Data.Common;

string inputFile = "./TempFuktData.csv";
List<WeatherModel> outputRecords = new();

using var reader = new StreamReader(inputFile);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

var records = csv.GetRecords<WeatherModel>();



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
        // counter++;
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

static void readProduct()
{
    using var db = new EFContext();
    List<WeatherData> data = db.WeatherData.ToList();

    foreach (WeatherData d in data)
    {
        // Console.WriteLine($"ID: {d.Datum} | Name: {d.Plats}");
        System.Console.WriteLine($"{d.Datum} | {d.Plats} | {d.Datum} | {d.Luftfuktighet}");

    }
    return;
}

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
        Console.Write($"Record Added -- {data.Datum} | {data.Plats} | {data.Temp} | {data.Luftfuktighet}");
    }
    else 
    {

        List<WeatherData> dataList = db.WeatherData.ToList();
        for (int i=0; i <= dataList.Count; i++)
        {
            Console.Write("\r Record {0} of {1} already exists", i, dataList.Count);
            Thread.Sleep(10);
        }
    }

}


