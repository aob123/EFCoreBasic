using Microsoft.EntityFrameworkCore;

namespace EFCOREBASIC.Models;

public class EFContext : DbContext
{
    private const string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EFCore;Trusted_Connection=True;";

     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
     {
             optionsBuilder.UseSqlServer(connectionString);
     }

        // protected override void OnConfiguring(DbContextOptionsBuilder options)
         //   => options.UseSqlServer("Server=localhost,1433; Database=NewDatabase;User=SA; Password=reallyStrongPwd123; TrustServerCertificate=True");
    public DbSet<WeatherData> WeatherData { get; set; }
    // public DbSet<Products> Products { get; set; }
    
}