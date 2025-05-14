using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using HCSSystem.Data;
using HCSSystem.Helpers;
using HCSSystem.Views;
using Microsoft.EntityFrameworkCore;

namespace HCSSystem.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Login(object obj)
        {
            using var db = new HcsDbContext();

            var user = db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == Username);

            if (user == null || !user.IsActive)
            {
                MessageBox.Show("Аккаунт не найден или не активен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var hash = HashPassword(Password);
            if (user.PasswordHash != hash)
            {
                MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Password == "qwe123")
            {
                var changePasswordDialog = new ChangePasswordDialog(user);
                changePasswordDialog.ShowDialog();

                // если пользователь не сменил пароль — не пускать
                if (!changePasswordDialog.PasswordChanged)
                    return;
            }

            App.CurrentUser = user; 
            new MainWindow().Show();
            CloseWindow(obj);
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private void Cancel(object obj)
        {
            CloseWindow(obj);
        }

        private void CloseWindow(object obj)
        {
            if (obj is Window window)
                window.Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}