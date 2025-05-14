using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using System.Windows;

namespace HCSSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User? CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var db = new HcsDbContext();

            //if (!db.Addresses.Any())
            //{
            //    TestDataGenerator.SeedAddressesAndOwnerships();
            //}
            //if (!db.Users.Any())
            //{
            //    TestUserDataSeeder.SeedUsersAndProfiles();
            //    ClientAddressSeeder.AssignClientsToAddresses();
            //}
            //DbSeeder.SeedUnitsOfMeasurement();
            //DbSeeder.SeedServices();
            //DbSeeder.Test();
            //DbSeeder.DeleteServices();
            //DbSeeder.SeedRates();
            //DbSeeder.SeedMetersAndReadings();
        }
    }

}
