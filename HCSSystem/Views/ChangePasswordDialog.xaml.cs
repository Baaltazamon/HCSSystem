using HCSSystem.Data;
using HCSSystem.Data.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace HCSSystem.Views
{
    public partial class ChangePasswordDialog : Window
    {
        private readonly User _user;
        public bool PasswordChanged { get; private set; } = false;

        public ChangePasswordDialog(User user)
        {
            InitializeComponent();
            _user = user;
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            var newPassword = NewPasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать минимум 4 символа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new HcsDbContext();
            var userInDb = db.Users.Find(_user.Id);
            if (userInDb != null)
            {
                userInDb.PasswordHash = HashPassword(newPassword);
                db.SaveChanges();
                PasswordChanged = true;
                this.Close();
            }
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}