using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EFCOREBASIC;

namespace EFCoreBasic
{
    public class MenuUI
    {
        public static void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Huvudmeny:");
                Console.WriteLine("1. Inomhusdata");
                Console.WriteLine("2. Utomhusdata");
                Console.WriteLine("3. Avsluta");
                Console.Write("Välj ett alternativ: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        ShowIndoorMenu();
                        break;
                    case "2":
                        ShowOutdoorMenu();
                        break;
                    case "3":
                        ExitProgram();
                        return;
                    default:
                        Console.WriteLine("Ogiltigt val. Tryck valfri tangent för att försöka igen.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void ShowIndoorMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Inomhusmeny:");
                Console.WriteLine("1. Medeltemperatur för valt datum");
                Console.WriteLine("2. Sortering: Varmaste till kallaste dagen");
                Console.WriteLine("3. Sortering: Torraste till fuktigaste dagen");
                Console.WriteLine("4. Sortering: Minst till störst risk för mögel");
                Console.WriteLine("5. Tillbaka till huvudmenyn");
                Console.Write("Välj ett alternativ: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        DataMethods.GetAverageTemperature(true);
                        break;
                    case "2":
                        DataMethods.SortByTemperature(true);
                        break;
                    case "3":
                        DataMethods.SortByHumidity(true);
                        break;
                    case "4":
                        DataMethods.SortByMoldRisk(true);
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Ogiltigt val. Tryck valfri tangent för att försöka igen.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void ShowOutdoorMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Utomhusmeny:");
                Console.WriteLine("1. Medeltemperatur för valt datum");
                Console.WriteLine("2. Sortering: Varmaste till kallaste dagen");
                Console.WriteLine("3. Sortering: Torraste till fuktigaste dagen");
                Console.WriteLine("4. Sortering: Minst till störst risk för mögel");
                Console.WriteLine("5. Datum för meteorologisk höst");
                Console.WriteLine("6. Datum för meteorologisk vinter");
                Console.WriteLine("7. Tillbaka till huvudmenyn");
                Console.Write("Välj ett alternativ: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        DataMethods.GetAverageTemperature(false);
                        break;
                    case "2":
                        DataMethods.SortByTemperature(false);
                        break;
                    case "3":
                        DataMethods.SortByHumidity(false);
                        break;
                    case "4":
                        DataMethods.SortByMoldRisk(false);
                        break;
                    case "5":
                        DataMethods.GetAutumnDate();
                        break;
                    case "6":
                        DataMethods.GetWinterDate();
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Ogiltigt val. Tryck valfri tangent för att försöka igen.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void ExitProgram()
        {
            Console.Clear();
            Console.WriteLine("Avslutar programmet...");
            Environment.Exit(0);
        }



        private static void ShowWeatherData()
        {
            Console.Clear();
            Console.WriteLine("Visar väderdata:");
            DataMethods.ShowWeatherData();
            GoBackToMenu();
        }

        private static void GoBackToMenu()
        {
            Console.WriteLine("Tryck på en tangent för att gå tillbaka till huvudmenyn...");
            Console.ReadKey();
            ShowMainMenu();
        }
    }
}