
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

namespace EFCOREBASIC.Models;

public class WeatherModel
{
    public DateTime Datum {get; set;}

    public string? Plats {get; set;}

    public string? Temp {get; set;}

    public int Luftfuktighet {get; set;}

}