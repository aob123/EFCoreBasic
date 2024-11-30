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


        /*public static void SortByMoldRisk(bool isIndoor)
        {
            using (var context = new EFContext())
            {
                var data = context.WeatherData
                    .Where(w => w.Plats.Contains(isIndoor ? "Inomhus" : "Ute"))  // Bestäm plats baserat på inomhus eller utomhus
                    .GroupBy(w => w.Datum.Date)  // Gruppera efter datum
                    .Select(g => new
                    {
                        Date = g.Key.Date,
                        AverageTemp = g.Average(w => w.Temp),
                        AverageHumidity = g.Average(w => w.Luftfuktighet)
                    })
                    .ToList();

                // Lägger till för att kontrollera om data går in och visas
                Console.WriteLine($"Antal inomhusdata: {data.Count}");

                var sortedData = data
                    .Select(d => new
                    {
                        d.Date,
                        d.AverageTemp,
                        d.AverageHumidity,
                        MoldRisk = CalculateMoldRisk((double)d.AverageTemp, (double)d.AverageHumidity)

                    })
                    .OrderByDescending(d => d.MoldRisk)  // Sortera efter mögelrisk (störst risk först)
                    .ToList();

                Console.WriteLine("Sortering efter mögelrisk (störst till minst risk):");
                foreach (var item in sortedData)
                {
                    Console.WriteLine($"{item.Date.ToShortDateString()} - Mögelrisk: {item.MoldRisk} (Temp: {item.AverageTemp}, Luftfuktighet: {item.AverageHumidity})");
                }

                Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                Console.ReadKey();
            }
        }*/

        //Sort mold outdoor
        public static void SortByMoldRiskOutdoors()
        {
            using (var context = new EFContext())
            {
                var data = context.WeatherData
                    .Where(w => w.Plats.Contains("Ute"))  // Bestäm plats baserat på inomhus eller utomhus
                    .GroupBy(w => w.Datum.Date)  // Gruppera efter datum
                    .Select(g => new
                    {
                        Date = g.Key.Date,
                        AverageTemp = g.Average(w => w.Temp),
                        AverageHumidity = g.Average(w => w.Luftfuktighet)
                    })
                    .ToList();

                // Lägger till för att kontrollera om data går in och visas
                Console.WriteLine($"Antal utomhusdata: {data.Count}");

                var sortedData = data
                    .Select(d => new
                    {
                        d.Date,
                        d.AverageTemp,
                        d.AverageHumidity,
                        MoldRisk = CalculateMoldRisk((double)d.AverageTemp, (double)d.AverageHumidity)

                    })
                    .OrderByDescending(d => d.MoldRisk)  // Sortera efter mögelrisk (störst risk först)
                    .ToList();

                Console.WriteLine("Sortering efter mögelrisk (störst till minst risk):");
                foreach (var item in sortedData)
                {
                    Console.WriteLine($"{item.Date.ToShortDateString()} - Mögelrisk: {item.MoldRisk} (Temp: {item.AverageTemp}, Luftfuktighet: {item.AverageHumidity})");
                }

                Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                Console.ReadKey();
            }
        }

        //Sort mold outdoor
        public static void SortByMoldRiskIndoor()
        {
            using (var context = new EFContext())
            {
                var data = context.WeatherData
                    .Where(w => w.Plats.Contains("Inne"))  // Bestäm plats baserat på inomhus eller utomhus
                    .GroupBy(w => w.Datum.Date)  // Gruppera efter datum
                    .Select(g => new
                    {
                        Date = g.Key.Date,
                        AverageTemp = g.Average(w => w.Temp),
                        AverageHumidity = g.Average(w => w.Luftfuktighet)
                    })
                    .ToList();

                // Lägger till för att kontrollera om data går in och visas
                Console.WriteLine($"Antal inomhusdata: {data.Count}");

                var sortedData = data
                    .Select(d => new
                    {
                        d.Date,
                        d.AverageTemp,
                        d.AverageHumidity,
                        MoldRisk = CalculateMoldRisk((double)d.AverageTemp, (double)d.AverageHumidity)

                    })
                    .OrderByDescending(d => d.MoldRisk)  // Sortera efter mögelrisk (störst risk först)
                    .ToList();

                Console.WriteLine("Sortering efter mögelrisk (störst till minst risk):");
                foreach (var item in sortedData)
                {
                    Console.WriteLine($"{item.Date.ToShortDateString()} - Mögelrisk: {item.MoldRisk} (Temp: {item.AverageTemp}, Luftfuktighet: {item.AverageHumidity})");
                }

                Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                Console.ReadKey();
            }
        }


        private static double CalculateMoldRisk(double temperature, double humidity)
        {
            if (humidity >= 70 && temperature >= 0 && temperature <= 30)
            {
                return (humidity - 70) * 0.1 + (temperature - 0) * 0.2;  // Skala risken
            }
            return 0;
        }


        public static void IndoorData()
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