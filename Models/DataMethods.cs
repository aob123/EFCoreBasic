using EFCOREBASIC.Models;
using CsvHelper;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
namespace EFCoreBasic
{

    class DataMethods
    {

        //Loads data from CSV to DB
        public static void AddWeatherData()
        {
            string inputFile = "C:\\Users\\alexh\\Source\\Repos\\EFCoreBasic\\TempFuktData.csv";
            List<WeatherModel> outputRecords = new List<WeatherModel>();

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

                var format = new NumberFormatInfo
                {
                    NegativeSign = "-", // Standard negativt tecken
                    NumberDecimalSeparator = "."
                };

                bool success = decimal.TryParse(record.Temp, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, format, out decimal result);

                if (success)
                {
                    insertData(record.Datum, record.Plats, result, record.Luftfuktighet);
                }
                else
                {
                    Console.WriteLine("Failed to parse the number.");
                }
            }
        }

        //Add record to db
        static void insertData(DateTime date, string location, decimal temp, int hum)
        {
            using var db = new EFContext();
            WeatherData data = new WeatherData
            {
                Datum = date,
                Plats = location,
                Temp = temp,
                Luftfuktighet = hum
            };

            if (!db.WeatherData.Contains(data))
            {
                db.Add(data);
                db.SaveChanges();
                Console.WriteLine($"Record Added -- {data.Datum} | {data.Plats} | {data.Temp} | {data.Luftfuktighet}");
            }
            else
            {
                Console.WriteLine($"Record already exists: {data.Datum} | {data.Plats} | {data.Temp} | {data.Luftfuktighet}");
            }
        }

        //Get all data from db
        public static void ShowWeatherData()
        {
            using var db = new EFContext();
            List<WeatherData> data = db.WeatherData.ToList();

            foreach (WeatherData d in data)
            {
                Console.WriteLine($"{d.Datum} | {d.Plats} | {d.Temp} | {d.Luftfuktighet}");
            }
        }

        //Get indoor data
        public static void IndoorData()
        {
            using (var context = new EFContext())
            {
                var data = context.WeatherData.Where(d => d.Plats.Contains("Inne")).ToList();

                foreach( WeatherData d in data)
                {
                    Console.WriteLine($"{d.Datum} | {d.Plats} | {d.Temp} | {d.Luftfuktighet}");
                }
            }
        }

        public static void IndoorAverageTemp()
        {
            using (var context = new EFContext())
            {
                Console.Write("Please type in the number for the month that you'd like to use [October/November]: ");
                int month = int.Parse(Console.ReadLine());
                int endMonth = month;
                if (month < 10 || month > 11)
                {
                    Console.WriteLine("Invalid input");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                Console.Write("Please type in the number for the day that you'd like to use [1-31/1-30]: ");
                int day = int.Parse(Console.ReadLine());
                int endDay = day + 1;
                if (month == 10 && day > 31 || month == 11 && day > 30)
                {
                    Console.WriteLine("Invalid input");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }
                else if (month == 10 && day == 31)
                {
                    endDay = 1;
                    endMonth = 11;
                }

                string monthString = month.ToString();
                string dayString = day.ToString();
                string endMonthString = endMonth.ToString();
                string endDayString = endDay.ToString();
                string chosenDate = "2016/" + monthString + "/" + dayString;
                string chosenEndDate = "2016/" + endMonthString + "/" + endDayString;

                var startDate = DateTime.ParseExact(chosenDate, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                var endDate = DateTime.ParseExact(chosenEndDate, "yyyy/MM/dd", CultureInfo.InvariantCulture);

                var data = context.WeatherData.Where(d => d.Plats.Contains("Inne") && d.Datum >= startDate && d.Datum <= endDate).ToList();

                decimal addedTemp = 0;
                int counter = 0;

                foreach (WeatherData d in data)
                {
                    addedTemp += d.Temp;
                    counter++;
                }

                decimal mediumTemp = addedTemp / counter;
                Console.WriteLine("Medeltemperaturen för " + chosenDate + " är " + mediumTemp);
                Console.ReadKey();
            }
        }

        //Get all indoor average temperatures by day
        public static void AllIndoorAverageTemp()
        {
            using (var context = new EFContext())
            {

                var data = context.WeatherData
                    .Where(d => d.Plats.Contains("Inne"))
                    .GroupBy(d => d.Datum.Date)
                    .Select(group => new
                    {
                        group.Key.Date,
                        AverageTemp = group.Average(x => x.Temp)
                    })
                    .OrderByDescending(d => d.AverageTemp)
                    .ToList();

                foreach (var d in data)
                {
                    Console.WriteLine(d.Date + " " + d.AverageTemp);
                }

                Console.ReadKey(true);
            }
        }

        //Get outdoor data
        public static void OutdoorData()
        {
            using (var context = new EFContext())
            {
                var data = context.WeatherData.Where(d => d.Plats.Contains("Ute")).ToList();

                foreach( WeatherData d in data)
                {
                    Console.WriteLine($"{d.Datum} | {d.Plats} | {d.Temp} | {d.Luftfuktighet}");
                }
            }
        }

        
    }

}