using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using HCSSystem.ViewModels.Models;
using HCSSystem.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels
{
    public class EmployeesViewModel : INotifyPropertyChanged
    {
        private string _searchQuery;
        private Role _selectedRole;

        public ObservableCollection<UserViewModel> FilteredEmployees { get; set; } = new();
        public ObservableCollection<Role> Roles { get; set; } = new();

        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public Role SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        private List<UserViewModel> _allEmployees = new();
        public ICommand BlockUserCommand { get; }

        public EmployeesViewModel()
        {
            AddCommand = new RelayCommand(_ => AddEmployee());
            EditCommand = new RelayCommand(emp => EditEmployee(emp as UserViewModel));
            DeleteCommand = new RelayCommand(emp => DeleteEmployee(emp as UserViewModel));
            BlockUserCommand = new RelayCommand(clientObj => BlockUser(clientObj as UserViewModel));
            LoadRoles();
            LoadEmployees();
        }

        private void BlockUser(UserViewModel? client)
        {
            if (client == null) return;
            using var db = new HcsDbContext();
            var user = db.Users.FirstOrDefault(u => u.Id == client.Id);
            if (user == null) return;

            var word = user.IsActive ? "Заблокировать" : "Разблокировать";

            var result = MessageBox.Show($"{word} сотрудника {client.Username}?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;



            user.IsActive = !user.IsActive;
            db.SaveChanges();

            LoadEmployees();
        }

        private void LoadRoles()
        {
            using var db = new HcsDbContext();
            Roles.Clear();
            foreach (var role in db.Roles.Where(c=> c.Id != 3).ToList())
                Roles.Add(role);
        }

        private void LoadEmployees()
        {
            using var db = new HcsDbContext();
            _allEmployees = db.Employees
                .Include(e => e.User)
                .ThenInclude(u => u.Role)
                .Select(e => new UserViewModel
                {
                    Id = e.Id,
                    Username = e.User.Username,
                    FullName = e.LastName + " " + e.FirstName + " " + e.MiddleName,
                    Email = e.Email,
                    Phone = e.PhoneNumber,
                    BirthDate = e.BirthDate,
                    PhotoFileName = e.PhotoFileName,
                    RoleName = e.User.Role.Name,
                    IsActive = e.User.IsActive
                })
                .ToList();

            ApplyFilter();
        }

        

        private void ApplyFilter()
        {
            var filtered = _allEmployees.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(e =>
                    (e.Username != null && e.Username.ToLower().Contains(query)) ||
                    (e.FullName != null && e.FullName.ToLower().Contains(query)) ||
                    (e.Email != null && e.Email.ToLower().Contains(query)) ||
                    (e.Phone != null && e.Phone.ToLower().Contains(query)));
            }

            if (SelectedRole != null)
            {
                filtered = filtered.Where(e => e.RoleName == SelectedRole.Name);
            }

            FilteredEmployees.Clear();
            foreach (var emp in filtered)
                FilteredEmployees.Add(emp);
        }

        private void AddEmployee()
        {
            var vm = new AddEditEmployeeDialogViewModel { IsEditMode = false };
            var dialog = new AddEditEmployeeDialog
            {
                DataContext = vm
            };

            vm.CloseRequested += () =>
            {
                dialog.Close();
                LoadEmployees();
            };

            dialog.ShowDialog();
        }

        private void EditEmployee(UserViewModel? user)
        {
            if(user == null) return;

            using var db = new HcsDbContext();
            var entity = db.Employees.Include(e => e.User).FirstOrDefault(e => e.Id == user.Id);
            if (entity == null) return;

            var vm = new AddEditEmployeeDialogViewModel
            {
                IsEditMode = true,
                Id = entity.Id,
                Username = entity.User.Username,
                LastName = entity.LastName,
                FirstName = entity.FirstName,
                MiddleName = entity.MiddleName,
                BirthDate = entity.BirthDate,
                Phone = entity.PhoneNumber,
                Email = entity.Email,
                PhotoFileName = entity.PhotoFileName,
                SelectedRole = entity.User.Role
            };

            var dialog = new AddEditEmployeeDialog { DataContext = vm };

            vm.CloseRequested += () =>
            {
                dialog.Close();
                LoadEmployees();
            };

            dialog.ShowDialog();
        }

        private void DeleteEmployee(UserViewModel? user)
        {
            if (user == null) return;

            var result = MessageBox.Show($"Удалить {user.FullName}?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            using var db = new HcsDbContext();

            var entity = db.Employees.Include(e => e.User).FirstOrDefault(e => e.Id == user.Id);
            if (entity == null)
            {
                MessageBox.Show("Сотрудник не найден");
                return;
            }

            try
            {
                db.Users.Remove(entity.User);
                db.Employees.Remove(entity);
                db.SaveChanges();
                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Невозможно удалить сотрудника: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}