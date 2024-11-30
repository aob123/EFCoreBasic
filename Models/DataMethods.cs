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

            // Kontrollera om rekordet redan finns baserat på primärnycklar
            bool exists = db.WeatherData.Any(w => w.Datum == date && w.Plats == location && w.Luftfuktighet == hum);

            if (!exists)
            {
                db.Add(data);
                db.SaveChanges();
                Console.WriteLine($"Record Added -- {data.Datum} | {data.Plats} | {data.Temp}°C | {data.Luftfuktighet}%");
            }
            else
            {
                Console.WriteLine($"Record already exists: {data.Datum} | {data.Plats} | {data.Temp}°C | {data.Luftfuktighet}%");
            }
        }

        public static void ShowWeatherData()
        {
            using var db = new EFContext();
            List<WeatherData> data = db.WeatherData.ToList();

            foreach (WeatherData d in data)
            {
                Console.WriteLine($"{d.Datum:yyyy-MM-dd} | {d.Plats} | {d.Temp}°C | {d.Luftfuktighet}%");
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
                        Console.WriteLine($"Medeltemperatur för {date.ToShortDateString()} är: {avgTemp:F1}°C");
                    }
                    else
                    {
                        Console.WriteLine("Inga data hittades för det angivna datumet.");
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
                // Hämta data baserat på om det är inne eller ute
                var data = isIndoor
                    ? context.WeatherData.Where(d => d.Plats.Contains("Inne"))
                    : context.WeatherData.Where(d => d.Plats.Contains("Ute"));

                // Gruppera efter datum och beräkna medeltemperatur för varje datum
                var groupedData = data
                    .GroupBy(d => d.Datum.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        AverageTemp = g.Average(d => d.Temp)
                    })
                    .OrderBy(g => g.Date); // Sortera resultaten efter datum

                // Visa resultaten
                Console.WriteLine("Medeltemperaturer för varje dag:");
                foreach (var item in groupedData)
                {
                    Console.WriteLine($"{item.Date:yyyy-MM-dd} | Medeltemperatur: {item.AverageTemp:F1}°C");
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
                    Console.WriteLine($"{item.Datum:yyyy-MM-dd} | {item.Plats} | {item.Temp}°C | {item.Luftfuktighet}%");
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
                    Console.WriteLine($"{item.Datum:yyyy-MM-dd} | {item.Plats} | {item.Temp}°C | {item.Luftfuktighet}%");
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

                Console.WriteLine("Visar data sorterat efter mögelrisk:");
                foreach (var item in sortedData)
                {
                    Console.WriteLine($"{item.Datum:yyyy-MM-dd} | {item.Plats} | {item.Temp}°C | {item.Luftfuktighet}% | Mögelrisk: {item.MoldRisk:F2}");
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
                Console.WriteLine($"Meteorologisk höst startade den: {autumnStart.Value:yyyy-MM-dd}");
            }
            else
            {
                Console.WriteLine("Ingen meteorologisk höst hittades.");
            }
        }*/


        public static void GetAutumnDate()
        {
            using var context = new EFContext();
            var threshold = 10; // Tröskelvärde för höst

            var dates = context.WeatherData
                               .Where(w => w.Plats.Contains("Ute") && w.Temp < threshold)
                               .OrderBy(w => w.Datum)
                               .Select(w => w.Datum.Date)
                               .Distinct()
                               .ToList();

            DateTime? autumnStart = null;
            for (int i = 0; i <= dates.Count - 5; i++)
            {
                // Kontrollera om det finns 5 på varandra följande dagar
                if (dates[i + 1] == dates[i].AddDays(1) &&
                    dates[i + 2] == dates[i].AddDays(2) &&
                    dates[i + 3] == dates[i].AddDays(3) &&
                    dates[i + 4] == dates[i].AddDays(4))
                {
                    autumnStart = dates[i]; // Datumet för höstens start
                    break;
                }
            }

            if (autumnStart.HasValue)
            {
                Console.WriteLine($"Meteorologisk höst startade den: {autumnStart.Value:yyyy-MM-dd}");
            }
            else
            {
                Console.WriteLine("Ingen meteorologisk höst hittades.");
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
                // Hämta väderdata för inomhus (Plats innehåller "Inne")
                var data = context.WeatherData.Where(d => d.Plats.Contains("Inne")).ToList();

                foreach (var d in data)
                {
                    Console.WriteLine($"{d.Datum:yyyy-MM-dd} | {d.Plats} | {d.Temp}°C | {d.Luftfuktighet}%");
                }
            }
        }

        public static void OutdoorData()
        {
            using (var context = new EFContext())
            {
                // Hämta väderdata för utomhus (Plats innehåller "Ute")
                var data = context.WeatherData.Where(d => d.Plats.Contains("Ute")).ToList();

                foreach (var d in data)
                {
                    Console.WriteLine($"{d.Datum:yyyy-MM-dd} | {d.Plats} | {d.Temp}°C | {d.Luftfuktighet}%");
                }
            }
        }*/

        //Get outdoor data
        public static void OutdoorData()
        {
            using (var context = new EFContext())
            {
                var data = context.WeatherData.Where(d => d.Plats.Contains("Ute")).ToList();

                foreach (WeatherData d in data)
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

    }

}