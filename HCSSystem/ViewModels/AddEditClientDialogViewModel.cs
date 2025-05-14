using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels
{
    public class AddEditClientDialogViewModel : INotifyPropertyChanged
    {
        public event Action CloseRequested;

        public bool IsEditMode { get; set; } = false;

        public int? Id { get; set; }
        public string Username { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDate { get; set; } = DateTime.Today;
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? PhotoFileName { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UploadPhotoCommand { get; }

        public AddEditClientDialogViewModel()
        {
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());
            UploadPhotoCommand = new RelayCommand(_ => UploadPhoto());
        }

        private void UploadPhoto()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.png)|*.jpg;*.png"
            };
            if (dialog.ShowDialog() == true)
            {
                var fileName = Path.GetFileName(dialog.FileName);
                var destDir = Path.Combine(Environment.CurrentDirectory, "Resources", "Images", "Users");
                var destPath = Path.Combine(destDir, fileName);

                // если файл уже существует, добавим случайный символ перед расширением
                if (File.Exists(destPath))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    var randomChar = Path.GetRandomFileName()[0];
                    fileName = $"{nameWithoutExt}_{randomChar}{ext}";
                    destPath = Path.Combine(destDir, fileName);
                }

                File.Copy(dialog.FileName, destPath);
                PhotoFileName = fileName;
                OnPropertyChanged(nameof(PhotoFileName));
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(MiddleName) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new HcsDbContext();

            if (IsEditMode && Id.HasValue)
            {
                var client = db.Clients.Include(c => c.User).FirstOrDefault(c => c.Id == Id);
                if (client == null) return;

                client.LastName = LastName;
                client.FirstName = FirstName;
                client.MiddleName = MiddleName;
                client.BirthDate = BirthDate;
                client.Email = Email;
                client.PhoneNumber = Phone;
                client.PhotoFileName = PhotoFileName;

                db.SaveChanges();
            }
            else
            {
                if (db.Users.Any(u => u.Username == Username))
                {
                    MessageBox.Show("Логин уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var user = new User
                {
                    Username = Username,
                    PasswordHash = HashPassword("qwe123"),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    RoleId = db.Roles.FirstOrDefault(r => r.Name.ToLower() == "клиент")?.Id ?? 3
                };
                db.Users.Add(user);
                db.SaveChanges();

                var client = new Client
                {
                    Id = user.Id,
                    LastName = LastName,
                    FirstName = FirstName,
                    MiddleName = MiddleName,
                    BirthDate = BirthDate,
                    Email = Email,
                    PhoneNumber = Phone,
                    PhotoFileName = PhotoFileName
                };
                db.Clients.Add(client);
                db.SaveChanges();
            }

            CloseRequested?.Invoke();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
