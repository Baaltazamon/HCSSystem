using HCSSystem.Data;
using HCSSystem.Helpers;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _fullName;
        private string _roleName;
        private string _avatarPath;
        private bool _isAdmin;
        private bool _isClient;
        private bool _isEmployeesVisible = true;
        private bool _isMenuExpanded = true;

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string RoleName
        {
            get => _roleName;
            set { _roleName = value; OnPropertyChanged(); }
        }

        public string AvatarPath
        {
            get => _avatarPath;
            set { _avatarPath = value; OnPropertyChanged(); }
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set { _isAdmin = value; OnPropertyChanged(); }
        }

        public bool IsClient
        {
            get => _isClient;
            set { _isClient = value; OnPropertyChanged(); }
        }

        public bool IsEmployeesVisible
        {
            get => _isEmployeesVisible;
            set { _isEmployeesVisible = value; OnPropertyChanged(); }
        }

        public bool IsMenuExpanded
        {
            get => _isMenuExpanded;
            set { _isMenuExpanded = value; OnPropertyChanged(); OnPropertyChanged(nameof(MenuWidth)); }
        }

        public GridLength MenuWidth => IsMenuExpanded ? new GridLength(250) : new GridLength(60);

        public ICommand ToggleMenuCommand { get; }

        public MainWindowViewModel()
        {
            ToggleMenuCommand = new RelayCommand(_ => ToggleMenu());
            LoadCurrentUser();
            ToggleMenu();
        }

        public void ToggleMenu()
        {
            IsMenuExpanded = !IsMenuExpanded;
        }

        private void LoadCurrentUser()
        {
            if (App.CurrentUser == null)
                return;

            using var db = new HcsDbContext();

            var user = db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == App.CurrentUser.Id);

            if (user == null) return;

            RoleName = user.Role?.Name ?? "";
            var lowerRole = RoleName.ToLower();
            IsAdmin = lowerRole == "администратор";
            IsClient = lowerRole == "клиент";
            IsEmployeesVisible = lowerRole == "администратор" || lowerRole == "сотрудник";

            if (IsClient)
            {
                var client = db.Clients.FirstOrDefault(c => c.Id == user.Id);
                if (client != null)
                {
                    FullName = $"{client.LastName} {client.FirstName} {client.MiddleName}";
                    SetAvatar(client.PhotoFileName);
                }
            }
            else
            {
                var employee = db.Employees.FirstOrDefault(e => e.Id == user.Id);
                if (employee != null)
                {
                    FullName = $"{employee.LastName} {employee.FirstName} {employee.MiddleName}";
                    SetAvatar(employee.PhotoFileName);
                }
            }
        }

        private void SetAvatar(string? fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var path = Path.Combine(Environment.CurrentDirectory, "Resources", "Images", "Users", fileName);
                if (File.Exists(path))
                {
                    AvatarPath = path;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
    
}
