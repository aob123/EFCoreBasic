using System;
using EFCOREBASIC.Models;

using CsvHelper;
using System.Globalization;
using System.Data.Common;
using System.IO;
using Microsoft.EntityFrameworkCore;


namespace EFCoreBasic
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new EFContext();
            context.Database.Migrate();
            MenuUI.ShowMainMenu();
            MenuUI.HandleMenuSelection();
        }
    }
}