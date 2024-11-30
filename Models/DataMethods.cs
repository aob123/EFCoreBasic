using EFCOREBASIC.Models;
using CsvHelper;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
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

        //Get sleected indoor average temperature
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
                Console.Write("\nPress any key to continue...");
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
                Console.Write("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        //Get all indoor average humidity by day
        public static void AllIndoorAveragerHumidity()
        {
            using (var context = new EFContext())
            {

                var data = context.WeatherData
                    .Where(d => d.Plats.Contains("Inne"))
                    .GroupBy(d => d.Datum.Date)
                    .Select(group => new
                    {
                        group.Key.Date,
                        AverageHumidity = group.Average(x => x.Luftfuktighet)
                    })
                    .OrderBy(d => d.AverageHumidity)
                    .ToList();

                foreach (var d in data)
                {
                    Console.WriteLine(d.Date + " " + d.AverageHumidity);
                }
                Console.Write("\nPress any key to continue...");
                Console.ReadKey();
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

        //Get sleected outdoor average temperature
        public static void OutdoorAverageTemp()
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

                var data = context.WeatherData.Where(d => d.Plats.Contains("Ute") && d.Datum >= startDate && d.Datum <= endDate).ToList();

                decimal addedTemp = 0;
                int counter = 0;

                foreach (WeatherData d in data)
                {
                    addedTemp += d.Temp;
                    counter++;
                }

                decimal mediumTemp = addedTemp / counter;
                Console.WriteLine("Medeltemperaturen för " + chosenDate + " är " + mediumTemp);
                Console.Write("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        //Get all outdoor average temperatures by day
        public static void AllOutdoorAverageTemp()
        {
            using (var context = new EFContext())
            {

                var data = context.WeatherData
                    .Where(d => d.Plats.Contains("Ute"))
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
                Console.Write("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        //Get all outdoor average temperatures by day
        public static void DateForFall()
        {
            using (var context = new EFContext())
            {

                var data = context.WeatherData
                    .Where(d => d.Plats.Contains("Ute"))
                    .GroupBy(d => d.Datum.Date)
                    .Select(group => new
                    {
                        group.Key.Date,
                        AverageTemp = group.Average(x => x.Temp)
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                int count = 0;

                foreach (var d in data)
                {
                    if (d.AverageTemp < 10)
                    {
                        count++;
                    }
                    else if (d.AverageTemp >= 10)
                    {
                        count = 0;
                    }

                    if (count == 5)
                    {
                        Console.WriteLine(d.Date + " is the metrological date for fall this year");
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        return;
                    }
                }
                Console.WriteLine("The requirments necessary for metrological fall isn't met within these dates");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        //Get all outdoor average temperatures by day
        public static void DateForWinter()
        {
            using (var context = new EFContext())
            {

                var data = context.WeatherData
                    .Where(d => d.Plats.Contains("Ute"))
                    .GroupBy(d => d.Datum.Date)
                    .Select(group => new
                    {
                        group.Key.Date,
                        AverageTemp = group.Average(x => x.Temp)
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                int count = 0;

                foreach (var d in data)
                {
                    if (d.AverageTemp <= 0)
                    {
                        count++;
                    }
                    else if (d.AverageTemp > 0)
                    {
                        count = 0;
                    }

                    if (count == 5)
                    {
                        Console.WriteLine(d.Date + " is the metrological date for winter this year");
                        Console.Write("\nPress any key to continue...");
                        Console.ReadKey();
                        return;
                    }
                }
                Console.WriteLine("The requirments necessary for metrological winter isn't met within these dates");
                Console.Write("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        //Get all outdoor average humidity by day
        public static void AllOutdoorAveragerHumidity()
        {
            using (var context = new EFContext())
            {

                var data = context.WeatherData
                    .Where(d => d.Plats.Contains("Ute"))
                    .GroupBy(d => d.Datum.Date)
                    .Select(group => new
                    {
                        group.Key.Date,
                        AverageHumidity = group.Average(x => x.Luftfuktighet)
                    })
                    .OrderBy(d => d.AverageHumidity)
                    .ToList();

                foreach (var d in data)
                {
                    Console.WriteLine(d.Date + " " + d.AverageHumidity);
                }
                Console.Write("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        //Mold calculator
        static void MoldCalculator(string[] args)
        {
            // Ensure there are enough arguments
            if (args.Length < 2)
            {
                Console.WriteLine("Please provide temperature and humidity as arguments");
                return;
            }

            // Parse command line arguments
            float temp = float.Parse(args[0]);
            float hum = float.Parse(args[1]);

            // Define the mtab array
            int[,] mtab = new int[,]
            {
            { 0, 0, 0, 0 },     // 0°
            { 0, 97, 98, 100 }, // 1°
            { 0, 95, 97, 100 }, // 2°
            { 0, 93, 95, 100 }, // 3°
            { 0, 91, 93, 98 },  // 4°
            { 0, 88, 92, 97 },  // 5°
            { 0, 87, 91, 96 },  // 6°
            { 0, 86, 91, 95 },  // 7°
            { 0, 84, 90, 95 },  // 8°
            { 0, 83, 89, 94 },  // 9°
            { 0, 82, 88, 93 },  // 10°
            { 0, 81, 88, 93 },  // 11°
            { 0, 81, 88, 92 },  // 12°
            { 0, 80, 87, 92 },  // 13°
            { 0, 79, 87, 92 },  // 14°
            { 0, 79, 87, 91 },  // 15°
            { 0, 79, 86, 91 },  // 16°
            { 0, 79, 86, 91 },  // 17°
            { 0, 79, 86, 90 },  // 18°
            { 0, 79, 85, 90 },  // 19°
            { 0, 79, 85, 90 },   // 20°
            { 0, 79, 85, 90 },   // 21°
            { 0, 79, 85, 89 },   // 22°
            { 0, 79, 84, 89 },   // 23°
            { 0, 79, 84, 89 },   // 24°
            { 0, 79, 84, 89 },   // 25°
            { 0, 79, 84, 89 },   // 26°
            { 0, 79, 83, 88 },   // 27°
            { 0, 79, 83, 88 },   // 28°
            { 0, 79, 83, 88 },   // 29°
            { 0, 79, 83, 88 },   // 30°
            { 0, 79, 83, 88 },   // 31°
            { 0, 79, 83, 88 },   // 32°
            { 0, 79, 82, 88 },   // 33°
            { 0, 79, 82, 87 },   // 34°
            { 0, 79, 82, 87 },   // 35°
            { 0, 79, 82, 87 },   // 36°
            { 0, 79, 82, 87 },   // 37°
            { 0, 79, 82, 87 },   // 38°
            { 0, 79, 82, 87 },   // 39°
            { 0, 79, 82, 87 },   // 40°
            { 0, 79, 81, 87 },   // 41°
            { 0, 79, 81, 87 },   // 42°
            { 0, 79, 81, 87 },   // 43°
            { 0, 79, 81, 87 },   // 44°
            { 0, 79, 81, 86 },   // 45°
            { 0, 79, 81, 86 },   // 46°
            { 0, 79, 81, 86 },   // 47°
            { 0, 79, 80, 86 },   // 48°
            { 0, 79, 80, 86 },   // 49°
            { 0, 79, 80, 86 }    // 50°
            };

            // Round temperature and humidity
            int rtemp = (int)Math.Round(temp);
            int rhum = (int)Math.Round(hum);
            int mindex = 0;

            // Determine mindex based on rounded temperature and humidity
            if (rtemp <= 0 || rtemp > 50)
            {
                mindex = 0;
            }
            else
            {
                for (int i = 1; i < 4; i++)
                {
                    if (rhum < mtab[rtemp, i])
                    {
                        mindex = i - 1;
                        break;
                    }
                    else
                    {
                        mindex = 3;
                    }
                }
            }

            // Print the result
            Console.WriteLine(mindex);
        }
    }
}