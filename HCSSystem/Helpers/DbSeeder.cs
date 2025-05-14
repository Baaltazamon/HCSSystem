using HCSSystem.Data;
using HCSSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HCSSystem.Helpers;

public static class DbSeeder
{
    public static void SeedUnitsOfMeasurement()
    {
        using var db = new HcsDbContext();

        var predefinedUnits = new List<string>
        {
            "м²",
            "чел.",
            "услуга",
            "руб."
        };

        var existingUnits = db.UnitOfMeasurements
            .Select(u => u.Name)
            .ToHashSet();

        var newUnits = predefinedUnits
            .Where(name => !existingUnits.Contains(name))
            .Select(name => new UnitOfMeasurement { Name = name })
            .ToList();

        if (newUnits.Any())
        {
            db.UnitOfMeasurements.AddRange(newUnits);
            db.SaveChanges();
        }
    }

    public static void SeedMetersAndReadings()
    {
        using var db = new HcsDbContext();

        var rnd = new Random();

        var services = db.Services.Where(s => !s.IsDeleted).ToList();
        var existingMeterNumbers = db.Meters.Select(m => m.MeterNumber).ToHashSet();
        int meterCounter = 100000;

        // Клиенты с привязанными адресами
        var clients = db.Clients
            .Include(c => c.ClientAddresses)
                .ThenInclude(ca => ca.Address)
            .ToList();

        foreach (var client in clients)
        {
            // только активные привязки на текущую дату
            var activeAddresses = client.ClientAddresses
                .Where(ca =>
                    ca.OwnershipStartDate <= DateTime.Today &&
                    (ca.OwnershipEndDate == null || ca.OwnershipEndDate >= DateTime.Today))
                .Select(ca => ca.Address)
                .ToList();

            foreach (var address in activeAddresses)
            {
                int meterCount = rnd.Next(1, 5); // от 1 до 4 счётчиков

                for (int i = 0; i < meterCount; i++)
                {
                    var service = services[rnd.Next(services.Count)];

                    string meterNumber;
                    do
                    {
                        meterNumber = (meterCounter++).ToString();
                    } while (existingMeterNumbers.Contains(meterNumber));

                    var meter = new Meter
                    {
                        AddressId = address.Id,
                        ServiceId = service.Id,
                        MeterNumber = meterNumber,
                        InstallationDate = new DateTime(2024, 12, 1),
                        IsDeleted = false
                    };

                    db.Meters.Add(meter);
                    db.SaveChanges(); // получить Id

                    decimal baseValue = rnd.Next(20, 100);
                    for (int month = 1; month <= 5; month++)
                    {
                        var reading = new MeterReading
                        {
                            MeterId = meter.Id,
                            ReadingDate = new DateTime(2025, month, 1),
                            Value = baseValue + 20 * (month - 1),
                            IsApproved = true,
                            ApprovedByUserId = null
                        };

                        db.MeterReadings.Add(reading);
                    }

                    db.SaveChanges();
                }
            }
        }
    }



    public static void SeedServices()
    {
        using var db = new HcsDbContext();

        if (db.Services.Any()) return;

        var services = new List<Service>
        {
            new() { Name = "Холодная вода", UnitOfMeasurementId = 1 },
            new() { Name = "Горячая вода", UnitOfMeasurementId = 1 },
            new() { Name = "Водоотведение", UnitOfMeasurementId = 1 },
            new() { Name = "Электроэнергия (день)", UnitOfMeasurementId = 2 },
            new() { Name = "Электроэнергия (ночь)", UnitOfMeasurementId = 2 },
            new() { Name = "Отопление", UnitOfMeasurementId = 3 },
            new() { Name = "Содержание жилья", UnitOfMeasurementId = 8 },
            new() { Name = "Уборка подъездов", UnitOfMeasurementId = 9 },
            new() { Name = "Вывоз мусора", UnitOfMeasurementId = 10 },
            new() { Name = "Обслуживание лифта", UnitOfMeasurementId = 10 },
            new() { Name = "Домофон", UnitOfMeasurementId = 10 },
            new() { Name = "Дератизация", UnitOfMeasurementId = 10 },
            new() { Name = "Поверка счётчика", UnitOfMeasurementId = 11},
            new() { Name = "Установка счётчика", UnitOfMeasurementId = 11 },
            new() { Name = "Пломбировка", UnitOfMeasurementId = 11 }
        };

        db.Services.AddRange(services);
        db.SaveChanges();
    }

    public static void Test()
    {
        using var db = new HcsDbContext();
        var unit = db.UnitOfMeasurements.ToList();
        int c = 1;
    }
    
    public static void DeleteServices()
    {
        using var db = new HcsDbContext();
        
        db.MeterReadings.RemoveRange(db.MeterReadings);
        db.Meters.RemoveRange(db.Meters);
        db.Rates.RemoveRange(db.Rates);
        db.Services.RemoveRange(db.Services);

        db.SaveChanges();
    }


    public static void SeedRates()
    {
        using var db = new HcsDbContext();

        var existingRates = db.Rates
            .Where(r => !r.IsDeleted)
            .Select(r => r.ServiceId)
            .ToHashSet();

        var today = new DateTime(2024, 1, 1);
        var user = db.Users.FirstOrDefault(c => c.RoleId == 1);
        var userId = user?.Id ?? 1; // fallback

        var servicePrices = new Dictionary<string, decimal>
        {
            ["Содержание жилья"] = 25.00m,
            ["Текущий ремонт"] = 10.00m,
            ["Вывоз мусора"] = 150.00m,
            ["Холодная вода (по нормативу)"] = 180.00m,
            ["Холодная вода (по счётчику)"] = 38.00m,
            ["Горячая вода"] = 145.00m,
            ["Водоотведение"] = 30.00m,
            ["Отопление"] = 1800.00m,
            ["Электроэнергия (день)"] = 5.50m,
            ["Электроэнергия (ночь)"] = 3.20m,
            ["Электроэнергия ОДН"] = 0.80m,
            ["Уборка подъездов"] = 4.00m,
            ["Дератизация / дезинсекция"] = 12.00m,
            ["Обслуживание лифта"] = 20.00m,
            ["Видеонаблюдение"] = 5.00m,
            ["Поверка счётчика"] = 500.00m,
            ["Установка/замена счётчика"] = 1000.00m,
            ["Пломбировка прибора учёта"] = 200.00m,
            ["Выдача справки"] = 150.00m,
            ["Техническое обслуживание оборудования"] = 7.00m
        };

        var services = db.Services.ToList();
        var newRates = new List<Rate>();

        foreach (var pair in servicePrices)
        {
            var service = services.FirstOrDefault(s => s.Name == pair.Key);
            if (service != null && !existingRates.Contains(service.Id))
            {
                newRates.Add(new Rate
                {
                    ServiceId = service.Id,
                    PricePerUnit = pair.Value,
                    EffectiveFrom = today,
                    CreatedByUserId = userId,
                    IsDeleted = false
                });
            }
        }

        if (newRates.Any())
        {
            db.Rates.AddRange(newRates);
            db.SaveChanges();
        }
    }

}