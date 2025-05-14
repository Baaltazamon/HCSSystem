using HCSSystem.Data;
using HCSSystem.Helpers;
using HCSSystem.ViewModels.Models;
using HCSSystem.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using HCSSystem.Data.Entities;

namespace HCSSystem.ViewModels
{
    public class ClientsViewModel : INotifyPropertyChanged
    {
        private string _searchQuery;
        public ObservableCollection<UserViewModel> FilteredClients { get; set; } = new();
        private List<UserViewModel> _allClients = new();

        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OpenClientAddressesCommand { get; }
        public ICommand BlockUserCommand { get; }
        public ClientsViewModel()
        {
            AddCommand = new RelayCommand(_ => AddClient());
            EditCommand = new RelayCommand(client => EditClient(client as UserViewModel));
            DeleteCommand = new RelayCommand(client => DeleteClient(client as UserViewModel));
            OpenClientAddressesCommand = new RelayCommand(OpenClientAddresses);
            BlockUserCommand = new RelayCommand(clientObj => BlockUser(clientObj as UserViewModel));
            LoadClients();
        }

        private void BlockUser(UserViewModel? client)
        {
            if (client == null) return;
            using var db = new HcsDbContext();
            var user = db.Users.FirstOrDefault(u => u.Id == client.Id);
            if (user == null) return;

            var word = user.IsActive ? "Заблокировать" : "Разблокировать";

            var result = MessageBox.Show($"{word} клиента {client.Username}?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            

            user.IsActive = !user.IsActive;
            db.SaveChanges();

            LoadClients();
        }

        private void LoadClients()
        {
            using var db = new HcsDbContext();
            _allClients = db.Clients
                .Include(c => c.User)
                .ThenInclude(u => u.Role)
                .Select(c => new UserViewModel
                {
                    Id = c.Id,
                    Username = c.User.Username,
                    FullName = c.LastName + " " + c.FirstName + " " + c.MiddleName,
                    Email = c.Email,
                    Phone = c.PhoneNumber,
                    BirthDate = c.BirthDate,
                    PhotoFileName = c.PhotoFileName,
                    RoleName = c.User.Role.Name,
                    IsActive = c.User.IsActive
                })
                .ToList();
            ApplyFilter();
        }

        private void OpenClientAddresses(object obj)
        {
            if (obj is UserViewModel user)
            {
                var dialog = new ClientAddressesDialog(user.Id);
                dialog.ShowDialog();
            }
        }

        private void ApplyFilter()
        {
            var filtered = _allClients.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(c =>
                    (c.Username != null && c.Username.ToLower().Contains(query)) ||
                    (c.FullName != null && c.FullName.ToLower().Contains(query)) ||
                    (c.Email != null && c.Email.ToLower().Contains(query)) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(query)));
            }

            FilteredClients.Clear();
            foreach (var c in filtered)
                FilteredClients.Add(c);
        }

        private void AddClient()
        {
            var vm = new AddEditClientDialogViewModel { IsEditMode = false };
            var dialog = new AddEditClientDialog
            {
                DataContext = vm
            };

            vm.CloseRequested += () =>
            {
                dialog.Close();
                LoadClients();
            };

            dialog.ShowDialog();
        }

        private void EditClient(UserViewModel? user)
        {
            if (user == null) return;

            using var db = new HcsDbContext();
            var entity = db.Clients.Include(c => c.User).FirstOrDefault(c => c.Id == user.Id);
            if (entity == null) return;

            
            var vm = new AddEditClientDialogViewModel
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
                PhotoFileName = entity.PhotoFileName
            };
            var dialog = new AddEditClientDialog{DataContext = vm};
            vm.CloseRequested += () =>
            {
                dialog.Close();
                LoadClients();
            };
            dialog.ShowDialog();
        }

        private void DeleteClient(UserViewModel? user)
        {
            if (user == null) return;

            var result = MessageBox.Show($"Удалить {user.FullName}?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            using var db = new HcsDbContext();
            var entity = db.Clients.Include(c => c.User).FirstOrDefault(c => c.Id == user.Id);
            if (entity == null)
            {
                MessageBox.Show("Клиент не найден");
                return;
            }

            try
            {
                db.Users.Remove(entity.User);
                db.Clients.Remove(entity);
                db.SaveChanges();
                LoadClients();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Невозможно удалить клиента: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
