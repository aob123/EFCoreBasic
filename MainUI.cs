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
                Console.WriteLine("2. Medeltemperaturer per dag");
                Console.WriteLine("3. Medelluftfuktighet per dag");
                Console.WriteLine("4. Sortering: Minst till störst risk för mögel");
                Console.WriteLine("5. Tillbaka till huvudmenyn");
                Console.Write("Välj ett alternativ: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        DataMethods.GetAverageTemperature(true);  // true för inomhus
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
                        break;
                    case "2":
                        DataMethods.AllIndoorAverageTemp();
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
                        break;
                    case "3":
                        DataMethods.AllIndoorAveragerHumidity();
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
                        break;
                    case "4":
                        DataMethods.SortByMoldRiskIndoor();
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
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
                Console.WriteLine("2. Medeltemperaturer per dag");
                Console.WriteLine("3. Medelluftfuktighet per dag");
                Console.WriteLine("4. Meteorologisk höst");
                Console.WriteLine("5. Meteorologisk vinter");
                Console.WriteLine("6. Sortering: Minst till störst risk för mögel");
                Console.WriteLine("7. Tillbaka till huvudmenyn");
                Console.Write("Välj ett alternativ: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        DataMethods.GetAverageTemperature(false);  // false för utomhus
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
                        break;
                    case "2":
                        DataMethods.AllOutdoorAverageTemp();
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
                        break;
                    case "3":
                        DataMethods.AllOutdoorAveragerHumidity();
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
                        break;
                    case "4":
                        DataMethods.DateForFall();
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
                        break;
                    case "5":
                        DataMethods.DateForWinter();
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
                        break;
                    case "6":
                        DataMethods.SortByMoldRiskOutdoors();
                        Console.WriteLine("\nTryck på en tangent för att återgå till menyn...");
                        Console.ReadKey();
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
    }
}