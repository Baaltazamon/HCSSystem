using HCSSystem.Data.Entities;
using HCSSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSSystem.Helpers
{
    public static class TestDataGenerator
    {
        public static void SeedAddressesAndOwnerships()
        {
            var rand = new Random();

            var cities = new[]
            {
        "Москва", "Санкт-Петербург", "Казань", "Екатеринбург", "Новосибирск",
        "Пермь", "Челябинск", "Нижний Новгород", "Самара", "Ростов-на-Дону"
    };

            var streets = new[]
            {
        "Ленина", "Победы", "Гагарина", "Мира", "Центральная", "Советская", "Молодёжная",
        "Зелёная", "Новая", "Школьная", "Парковая", "Садовая", "Лесная", "Спортивная",
        "Берёзовая", "Полевая", "Озерная", "Трудовая", "Железнодорожная", "Набережная"
    };

            using var db = new HcsDbContext();

            var addresses = new List<Address>();

            foreach (var city in cities)
            {
                foreach (var street in streets)
                {
                    int housesCount = rand.Next(5, 11); // 5–10 домов
                    for (int h = 1; h <= housesCount; h++)
                    {
                        string house = h.ToString();

                        int apartmentsCount = rand.Next(70, 201); // 70–200 квартир
                        for (int apt = 1; apt <= apartmentsCount; apt++)
                        {
                            addresses.Add(new Address
                            {
                                City = city,
                                Street = street,
                                HouseNumber = house,
                                ApartmentNumber = apt.ToString(),
                                PropertyArea = rand.Next(30, 121), // 30–120 м²
                                IsResidential = rand.NextDouble() > 0.2, // 80% жилое
                                IsDeleted = false,
                                Building = rand.NextDouble() < 0.3 ? $"к{rand.Next(1, 4)}" : null, 
                            });
                        }
                    }
                }
            }

            db.Addresses.AddRange(addresses);
            db.SaveChanges();

            // Связь с клиентами
            var allAddresses = db.Addresses.ToList();
            var clients = db.Clients.ToList();

            foreach (var client in clients)
            {
                int count = rand.Next(1, 4); // 1–3 адреса
                var selected = allAddresses.OrderBy(_ => rand.Next()).Take(count).ToList();

                foreach (var address in selected)
                {
                    db.ClientAddresses.Add(new ClientAddress
                    {
                        ClientId = client.Id,
                        AddressId = address.Id,
                        OwnershipStartDate = DateTime.Today.AddYears(-rand.Next(1, 5)),
                        OwnershipEndDate = null,
                        PersonalAccountNumber = Guid.NewGuid().ToString(),
                    });
                }
            }

            db.SaveChanges();
        }

    }
}