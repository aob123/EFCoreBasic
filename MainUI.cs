using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EFCoreBasic
{
    public class MenuUI
    {
        public static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("Välj ett alternativ:");
            Console.WriteLine("1. Lägg till väderdata");
            Console.WriteLine("2. Visa väderdata");
            Console.WriteLine("3. Sortera väderdata efter temperatur");
            Console.WriteLine("4. Sortera väderdata efter luftfuktighet");
            Console.WriteLine("5. Avsluta");
        }

        public static void HandleMenuSelection()
        {
            string userChoice = Console.ReadLine();

            switch (userChoice)
            {
                case "1":
                    AddWeatherData();
                    break;
                case "2":
                    ShowWeatherData();
                    break;
                case "3":
                    SortDataByTemperature();
                    break;
                case "4":
                    SortDataByHumidity();
                    break;
                case "5":
                    ExitProgram();
                    break;
                default:
                    Console.WriteLine("Ogiltigt val, försök igen.");
                    break;
            }
        }

        private static void AddWeatherData()
        {
            Console.Clear();
            Console.WriteLine("Lägg till ny väderdata");

            // (Datum, Plats, Temperatur, Luftfuktighet)
            // sedan anropa insertData metod
            // Ex:
            // insertData(DateTime.Now, "Stockholm", 10.5m, 75);

            Console.WriteLine("Data har lagts till.");
            GoBackToMenu();
        }

        private static void ShowWeatherData()
        {
            Console.Clear();
            Console.WriteLine("Visar väderdata:");



            GoBackToMenu();
        }

        private static void SortDataByTemperature()
        {
            Console.Clear();
            Console.WriteLine("Sorterar väderdata efter temperatur:");

            // anropa logik för att sortera och visa väderdata baserat på temperatur
            // Ex:
            // SortWeatherDataByTemperature();

            GoBackToMenu();
        }

        private static void SortDataByHumidity()
        {
            Console.Clear();
            Console.WriteLine("Sorterar väderdata efter luftfuktighet:");

            // anropa logik för att sortera och visa väderdata baserat på luftfuktighet
            // Ex:
            // SortWeatherDataByHumidity();

            GoBackToMenu();
        }

        private static void GoBackToMenu()
        {
            Console.WriteLine("Tryck på en tangent för att gå tillbaka till huvudmenyn...");
            Console.ReadKey();
            ShowMainMenu();
            HandleMenuSelection();
        }

        private static void ExitProgram()
        {
            Console.Clear();
            Console.WriteLine("Avslutar programmet...");
            Environment.Exit(0);
        }
    }
}
