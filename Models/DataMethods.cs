using EFCOREBASIC.Models;
using CsvHelper;
using System.Globalization;
namespace EFCoreBasic
{

    class DataMethods
    {

        //Loads data from CSV to DB
        static void AddWeatherData()
        {
            string inputFile = "./TempFuktData.csv";
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