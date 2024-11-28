
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace EFCOREBASIC.Models;
[PrimaryKey(nameof(Datum), nameof(Plats), nameof(Luftfuktighet))]
public class WeatherData
{
    // public int ID {get; set;}
    
    public DateTime Datum {get; set;}

    public string? Plats {get; set;}

    public decimal Temp {get; set;}

    public int Luftfuktighet {get; set;}

}