using System;
using EFCOREBASIC.Models;

using CsvHelper;
using System.Globalization;
using System.Data.Common;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace EFCoreBasic
{
    class Program
    {
        static void Main(string[] args)
        {
            // Laddar in nedan AddWeatherData, kommenterar sedan bort
            // för att undvika dubblett inläsning

            // DataMethods.AddWeatherData();
            var context = new EFContext();
            context.Database.Migrate();
            MenuUI.ShowMainMenu();
            // MenuUI.HandleMenuSelection();
        }
    }
}