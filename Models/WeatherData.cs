
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

namespace EFCOREBASIC.Models;

public class WeatherData
{
    public int ID {get; set;}
    public DateTime Datum {get; set;}

    public string? Plats {get; set;}

    // [TypeConverter(typeof(DecimalConverter))]
    public decimal Temp {get; set;}

    public int Luftfuktighet {get; set;}

}