using EFCOREBASIC.Models;
using CsvHelper;
using System.Globalization;
namespace EFCoreBasic
{

    public class DataMethods
    {
        // Loads data from CSV to DB
        public static void AddWeatherData()
        {
            //string inputFile = "./TempFuktData.csv";
            string inputFile = "C:\\Users\\magnu\\source1\\TempFuktData.csv";
            // var read = new StreamReader("C:\\Users\\magnu\\source1\\TempFuktData.csv");


            try
            {
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
                        NegativeSign = "-", // Define standard negative sign
                        NumberDecimalSeparator = "."
                    };

                    bool success = decimal.TryParse(record.Temp, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, format, out decimal result);

                    if (success)
                    {
                        InsertData(record.Datum, record.Plats, result, record.Luftfuktighet);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse Temp for record: {record.Datum}, {record.Plats}");
                    }
                }

                Console.WriteLine("Data importerad.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV file: {ex.Message}");
            }
        }

        public static void InsertData(DateTime date, string location, decimal temp, int hum)
        {
            using var db = new EFContext();
            WeatherData data = new WeatherData
            {
                Datum = date,
                Plats = location,
                Temp = temp,
                Luftfuktighet = hum
            };

            // Kontrollera om rekordet redan finns baserat p� prim�rnycklar
            bool exists = db.WeatherData.Any(w => w.Datum == date && w.Plats == location && w.Luftfuktighet == hum);

            if (!exists)
            {
                db.Add(data);
                db.SaveChanges();
                Console.WriteLine($"Record Added -- {data.Datum} | {data.Plats} | {data.Temp}�C | {data.Luftfuktighet}%");
            }
            else
            {
                Console.WriteLine($"Record already exists: {data.Datum} | {data.Plats} | {data.Temp}�C | {data.Luftfuktighet}%");
            }
        }

        public static void ShowWeatherData()
        {
            using var db = new EFContext();
            List<WeatherData> data = db.WeatherData.ToList();

            foreach (WeatherData d in data)
            {
                Console.WriteLine($"{d.Datum:yyyy-MM-dd} | {d.Plats} | {d.Temp}�C | {d.Luftfuktighet}%");
            }
        }


        public static void GetAverageTemperature(bool isIndoor)
        {
            Console.WriteLine("Ange ett datum (yyyy-MM-dd):");
            string dateInput = Console.ReadLine();
            if (DateTime.TryParse(dateInput, out DateTime date))
            {
                using (var context = new EFContext())
                {
                    var data = isIndoor
                        ? context.WeatherData.Where(d => d.Plats.Contains("Inne") && d.Datum.Date == date.Date)
                        : context.WeatherData.Where(d => d.Plats.Contains("Ute") && d.Datum.Date == date.Date);

                    if (data.Any())
                    {
                        var avgTemp = data.Average(d => d.Temp);
                        Console.WriteLine($"Medeltemperatur f�r {date.ToShortDateString()} �r: {avgTemp:F1}�C");
                    }
                    else
                    {
                        Console.WriteLine("Inga data hittades f�r det angivna datumet.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Ogiltigt datumformat.");
            }
        }

        /*public static void GetAverageTemperature(bool isIndoor)
        {
            using (var context = new EFContext())
            {
                // H�mta data baserat p� om det �r inne eller ute
                var data = isIndoor
                    ? context.WeatherData.Where(d => d.Plats.Contains("Inne"))
                    : context.WeatherData.Where(d => d.Plats.Contains("Ute"));

                // Gruppera efter datum och ber�kna medeltemperatur f�r varje datum
                var groupedData = data
                    .GroupBy(d => d.Datum.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        AverageTemp = g.Average(d => d.Temp)
                    })
                    .OrderBy(g => g.Date); // Sortera resultaten efter datum

                // Visa resultaten
                Console.WriteLine("Medeltemperaturer f�r varje dag:");
                foreach (var item in groupedData)
                {
                    Console.WriteLine($"{item.Date:yyyy-MM-dd} | Medeltemperatur: {item.AverageTemp:F1}�C");
                }
            }
        }*/


        public static void SortByTemperature(bool isIndoor)
        {
            using (var context = new EFContext())
            {
                var data = isIndoor
                    ? context.WeatherData.Where(d => d.Plats.Contains("Inne")).OrderByDescending(d => d.Temp)  // Inomhusdata
                    : context.WeatherData.Where(d => d.Plats.Contains("Ute")).OrderByDescending(d => d.Temp);  // Utomhusdata

                Console.WriteLine("Visar data sorterat efter temperatur:");
                foreach (var item in data)
                {
                    Console.WriteLine($"{item.Datum:yyyy-MM-dd} | {item.Plats} | {item.Temp}�C | {item.Luftfuktighet}%");
                }
            }
        }

        public static void SortByHumidity(bool isIndoor)
        {
            using (var context = new EFContext())
            {
                var data = isIndoor
                    ? context.WeatherData.Where(d => d.Plats.Contains("Inne")).OrderBy(d => d.Luftfuktighet)  // Inomhusdata
                    : context.WeatherData.Where(d => d.Plats.Contains("Ute")).OrderBy(d => d.Luftfuktighet);  // Utomhusdata

                Console.WriteLine("Visar data sorterat efter luftfuktighet:");
                foreach (var item in data)
                {
                    Console.WriteLine($"{item.Datum:yyyy-MM-dd} | {item.Plats} | {item.Temp}�C | {item.Luftfuktighet}%");
                }
            }
        }

        public static void SortByMoldRisk(bool isIndoor)
        {
            using (var context = new EFContext())
            {
                var data = isIndoor
                    ? context.WeatherData.Where(d => d.Plats.Contains("Inne"))
                    : context.WeatherData.Where(d => d.Plats.Contains("Ute"));

                var sortedData = data
                    .Select(d => new
                    {
                        d.Datum,
                        d.Plats,
                        d.Temp,
                        d.Luftfuktighet,
                        MoldRisk = (d.Luftfuktighet - 78) * (d.Temp / 15)
                    })
                    .OrderBy(d => d.MoldRisk);

                Console.WriteLine("Visar data sorterat efter m�gelrisk:");
                foreach (var item in sortedData)
                {
                    Console.WriteLine($"{item.Datum:yyyy-MM-dd} | {item.Plats} | {item.Temp}�C | {item.Luftfuktighet}% | M�gelrisk: {item.MoldRisk:F2}");
                }
            }
        }

        /*public static void GetAutumnDate()
        {
            using var context = new EFContext();
            var threshold = 10;

            var dates = context.WeatherData
                               .Where(w => w.Plats.Contains("Ute") && w.Temp < threshold)
                               .OrderBy(w => w.Datum)
                               .Select(w => w.Datum.Date)
                               .Distinct()
                               .ToList();

            DateTime? autumnStart = null;
            for (int i = 0; i <= dates.Count - 5; i++)
            {
                bool consecutive = true;
                for (int j = 0; j < 5; j++)
                {
                    if (dates[i + j].AddDays(j) != dates[i + j])
                    {
                        consecutive = false;
                        break;
                    }
                }

                if (consecutive)
                {
                    autumnStart = dates[i];
                    break;
                }
            }

            if (autumnStart.HasValue)
            {
                Console.WriteLine($"Meteorologisk h�st startade den: {autumnStart.Value:yyyy-MM-dd}");
            }
            else
            {
                Console.WriteLine("Ingen meteorologisk h�st hittades.");
            }
        }*/


        public static void GetAutumnDate()
        {
            using var context = new EFContext();
            var threshold = 10; // Tr�skelv�rde f�r h�st

            var dates = context.WeatherData
                               .Where(w => w.Plats.Contains("Ute") && w.Temp < threshold)
                               .OrderBy(w => w.Datum)
                               .Select(w => w.Datum.Date)
                               .Distinct()
                               .ToList();

            DateTime? autumnStart = null;
            for (int i = 0; i <= dates.Count - 5; i++)
            {
                // Kontrollera om det finns 5 p� varandra f�ljande dagar
                if (dates[i + 1] == dates[i].AddDays(1) &&
                    dates[i + 2] == dates[i].AddDays(2) &&
                    dates[i + 3] == dates[i].AddDays(3) &&
                    dates[i + 4] == dates[i].AddDays(4))
                {
                    autumnStart = dates[i]; // Datumet f�r h�stens start
                    break;
                }
            }

            if (autumnStart.HasValue)
            {
                Console.WriteLine($"Meteorologisk h�st startade den: {autumnStart.Value:yyyy-MM-dd}");
            }
            else
            {
                Console.WriteLine("Ingen meteorologisk h�st hittades.");
            }
        }


        public static void GetWinterDate()
        {
            using var context = new EFContext();
            var threshold = 0;

            var dates = context.WeatherData
                               .Where(w => w.Plats.Contains("Ute") && w.Temp < threshold)
                               .OrderBy(w => w.Datum)
                               .Select(w => w.Datum.Date)
                               .Distinct()
                               .ToList();

            DateTime? winterStart = null;
            for (int i = 0; i <= dates.Count - 5; i++)
            {
                bool consecutive = true;
                for (int j = 0; j < 5; j++)
                {
                    if (dates[i + j].AddDays(j) != dates[i + j])
                    {
                        consecutive = false;
                        break;
                    }
                }

                if (consecutive)
                {
                    winterStart = dates[i];
                    break;
                }
            }

            if (winterStart.HasValue)
            {
                Console.WriteLine($"Meteorologisk vinter startade den: {winterStart.Value:yyyy-MM-dd}");
            }
            else
            {
                Console.WriteLine("Ingen meteorologisk vinter hittades.");
            }
        }

        /*public static void IndoorData()
        {
            using (var context = new EFContext())
            {
                // H�mta v�derdata f�r inomhus (Plats inneh�ller "Inne")
                var data = context.WeatherData.Where(d => d.Plats.Contains("Inne")).ToList();

                foreach (var d in data)
                {
                    Console.WriteLine($"{d.Datum:yyyy-MM-dd} | {d.Plats} | {d.Temp}�C | {d.Luftfuktighet}%");
                }
            }
        }

        public static void OutdoorData()
        {
            using (var context = new EFContext())
            {
                // H�mta v�derdata f�r utomhus (Plats inneh�ller "Ute")
                var data = context.WeatherData.Where(d => d.Plats.Contains("Ute")).ToList();

                foreach (var d in data)
                {
                    Console.WriteLine($"{d.Datum:yyyy-MM-dd} | {d.Plats} | {d.Temp}�C | {d.Luftfuktighet}%");
                }
            }
        }*/

    }

}