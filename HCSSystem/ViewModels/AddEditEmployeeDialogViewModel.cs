using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels
{
    public class AddEditEmployeeDialogViewModel : INotifyPropertyChanged
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
        public ObservableCollection<Role> Roles { get; set; } = new();
        public Role SelectedRole { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UploadPhotoCommand { get; }

        public AddEditEmployeeDialogViewModel()
        {
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());
            UploadPhotoCommand = new RelayCommand(_ => UploadPhoto());
            LoadRoles();
        }

        private void LoadRoles()
        {
            using var db = new HcsDbContext();
            var roles = db.Roles.Where(r => r.Id != 3).ToList();
            Roles.Clear();
            foreach (var role in roles)
                Roles.Add(role);

            if (!IsEditMode)
                SelectedRole = Roles.FirstOrDefault();
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
            using var db = new HcsDbContext();

            if (IsEditMode && Id.HasValue)
            {
                var emp = db.Employees.Include(e => e.User).FirstOrDefault(e => e.Id == Id);
                if (emp == null) return;
                emp.LastName = LastName;
                emp.FirstName = FirstName;
                emp.MiddleName = MiddleName;
                emp.BirthDate = BirthDate;
                emp.Email = Email;
                emp.PhoneNumber = Phone;
                emp.PhotoFileName = PhotoFileName;
                emp.User.RoleId = SelectedRole.Id;
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
                    RoleId = SelectedRole?.Id ?? 2
                };
                db.Users.Add(user);
                db.SaveChanges();

                var emp = new Employee
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
                db.Employees.Add(emp);
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
