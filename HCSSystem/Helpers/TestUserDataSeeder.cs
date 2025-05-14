using HCSSystem.Data;
using HCSSystem.Data.Entities;
using System.Security.Cryptography;
using System.Text;

namespace HCSSystem.Helpers
{
    public static class TestUserDataSeeder
    {
        public static void SeedUsersAndProfiles()
        {
            using var db = new HcsDbContext();

            if (db.Users.Any()) return;

            var malePhotos = new[]
            {
                "2bcbd6fbfbb2020b59032c98f117.jpg",
                "6cdb3e7cdce61447367f8fe0822fa9de.jpg",
                "80bd0a412973120b0c6e081ea8b33582.jpg",
                "8264-59b73bd244ae2.jpg",
                "8530ea9014e1e8baf7857235a093eba5.jpg",
                "595390_1707979910_65cdb486847da.jpg",
                "2177236064_huge.jpg",
                "b8cf1db10884ccec7239496f3ee6c7dc.jpg",
                "kris.jpg",
                "man.jpg",
                "portrait-white-man-isolated-scaled-1.jpg",
                "qqqwww.jpg",
                "shutterstock_102443044.jpg",
                "wwweee.jpg"
            };

            var femalePhotos = new[]
            {
                "0dfb26368238f71d95d257f01002b8aa.jpg",
                "2d2e7b8c129740f9fdfcd341d0b151fc.jpg",
                "2155946130_huge.jpg",
                "b7d9b48ca8655b902b532c45d08400c7.jpg",
                "d05c92a4f55648cfd462ab4190b6d35c.png",
                "devushka-bryunetka-vzglyad-1704.jpg",
                "Elk4graljt4.jpg",
                "odeya-rush-ap-2932x2932.jpg",
                "qwqwqw.jpg",
                "wewewe.jpg"
            };

            var maleNames = new[] { "Алексей", "Иван", "Петр", "Сергей", "Максим", "Олег", "Роман", "Егор", "Виктор", "Андрей" };
            var femaleNames = new[] { "Елена", "Анна", "Ольга", "Наталья", "Мария", "Юлия", "Светлана", "Ирина", "Карина", "Ульяна" };
            var maleLastNames = new[] { "Иванов", "Петров", "Сидоров", "Смирнов", "Козлов", "Попов", "Кузнецов", "Новиков", "Тимофеев", "Чесноков" };
            var femaleLastNames = new[] { "Иванова", "Петрова", "Сидорова", "Смирнова", "Козлова", "Попова", "Кузнецова", "Новикова", "Тимофеева", "Чеснокова" };
            var maleMiddleNames = new[] { "Алексеевич", "Иванович", "Петрович", "Сергеевич", "Максимович", "Олегович", "Романович", "Егорович", "Викторович", "Андреевич" };
            var femaleMiddleNames = new[] { "Алексеевна", "Ивановна", "Петровна", "Сергеевна", "Максимовна", "Олеговна", "Романовна", "Егоровна", "Викторовна", "Андреевна" };

            var rand = new Random();

            int userCounter = 0;

            void AddEmployee(string login, string password, int roleId, string photoFile, string gender)
            {
                var role = db.Roles.FirstOrDefault(r => r.Id == roleId);
                if (role == null)
                    throw new InvalidOperationException($"Роль '{roleId}' не найдена. Убедитесь, что она добавлена в таблицу Roles.");

                var user = new User
                {
                    Username = login,
                    PasswordHash = HashPassword(password),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    RoleId = role.Id
                };
                db.Users.Add(user);
                db.SaveChanges();

                var employee = new Employee
                {
                    Id = user.Id,
                    LastName = gender == "male" ? maleLastNames[userCounter % maleLastNames.Length] : femaleLastNames[userCounter % femaleLastNames.Length],
                    FirstName = gender == "male" ? maleNames[userCounter % maleNames.Length] : femaleNames[userCounter % femaleNames.Length],
                    MiddleName = gender == "male" ? maleMiddleNames[userCounter % maleMiddleNames.Length] : femaleMiddleNames[userCounter % femaleMiddleNames.Length],
                    BirthDate = DateTime.Today.AddYears(-rand.Next(22, 45)),
                    PhoneNumber = $"+7 ({rand.Next(900, 1000)}) {rand.Next(1000000, 9999999)}",
                    Email = login + "@mail.ru",
                    PhotoFileName = photoFile
                };
                db.Employees.Add(employee);
                userCounter++;
            }

            void AddClient(string login, string password, string photoFile, string gender)
            {
                var user = new User
                {
                    Username = login,
                    PasswordHash = HashPassword(password),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    RoleId = db.Roles.First(r => r.Id == 3).Id
                };
                db.Users.Add(user);
                db.SaveChanges();

                var client = new Client
                {
                    Id = user.Id,
                    LastName = gender == "male" ? maleLastNames[userCounter % maleLastNames.Length] : femaleLastNames[userCounter % femaleLastNames.Length],
                    FirstName = gender == "male" ? maleNames[userCounter % maleNames.Length] : femaleNames[userCounter % femaleNames.Length],
                    MiddleName = gender == "male" ? maleMiddleNames[userCounter % maleMiddleNames.Length] : femaleMiddleNames[userCounter % femaleMiddleNames.Length],
                    BirthDate = DateTime.Today.AddYears(-rand.Next(18, 60)),
                    PhoneNumber = $"+7 ({rand.Next(900, 1000)}) {rand.Next(1000000, 9999999)}",
                    Email = login + "@mail.ru",
                    PhotoFileName = photoFile
                };
                db.Clients.Add(client);
                userCounter++;
            }

            // Добавляем сотрудников
            AddEmployee("admin", "1234", 1, "kris.jpg", "male");
            AddEmployee("admin1", "1234", 1, "man.jpg", "male");
            AddEmployee("empl", "1234", 2, "qqqwww.jpg", "male");
            AddEmployee("empl1", "1234", 2, "portrait-white-man-isolated-scaled-1.jpg", "male");
            AddEmployee("empl2", "1234", 2, "shutterstock_102443044.jpg", "male");

            // Остальные — клиенты
            int i = 0;
            foreach (var photo in malePhotos.Concat(femalePhotos))
            {
                if (photo == "kris.jpg" || photo == "man.jpg" || photo == "qqqwww.jpg" || photo == "portrait-white-man-isolated-scaled-1.jpg" || photo == "shutterstock_102443044.jpg")
                    continue;

                string login = "user" + (i == 0 ? "" : i.ToString());
                string gender = malePhotos.Contains(photo) ? "male" : "female";
                AddClient(login, "123", photo, gender);
                i++;
            }

            db.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
    }
}