using HCSSystem.Data;
using HCSSystem.Data.Entities;

namespace HCSSystem.Helpers
{
    public static class ClientAddressSeeder
    {
        public static void AssignClientsToAddresses()
        {
            using var db = new HcsDbContext();
            var rand = new Random();

            var clients = db.Clients.ToList();
            var addresses = db.Addresses.ToList();

            foreach (var client in clients)
            {
                int count = rand.Next(1, 4); // каждому клиенту от 1 до 3 адресов
                var selected = addresses.OrderBy(_ => rand.Next()).Take(count).ToList();

                foreach (var address in selected)
                {
                    if (!db.ClientAddresses.Any(ca => ca.ClientId == client.Id && ca.AddressId == address.Id))
                    {
                        db.ClientAddresses.Add(new ClientAddress
                        {
                            ClientId = client.Id,
                            AddressId = address.Id,
                            OwnershipStartDate = DateTime.Today.AddYears(-rand.Next(1, 6)),
                            OwnershipEndDate = null,
                            PersonalAccountNumber = Guid.NewGuid().ToString(),
                        });
                    }
                }
            }

            db.SaveChanges();
        }
    }
}