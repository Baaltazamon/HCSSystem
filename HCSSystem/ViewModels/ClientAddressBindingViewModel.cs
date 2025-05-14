using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using HCSSystem.ViewModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels
{
    public class ClientAddressBindingViewModel : INotifyPropertyChanged
    {
        private readonly int _clientId;

        public ObservableCollection<ClientAddressDto> BoundAddresses { get; set; } = new();
        public ObservableCollection<ClientAddressDto> AvailableAddresses { get; set; } = new();
        private string _addressSearchText;
        public string AddressSearchText
        {
            get => _addressSearchText;
            set
            {
                _addressSearchText = value;
                OnPropertyChanged();
                _ = SearchAddressesAsync();
            }
        }

        public ObservableCollection<ClientAddressDto> SearchResults { get; set; } = new();

        private ClientAddressDto _selectedAvailableAddress;
        public ClientAddressDto SelectedAvailableAddress
        {
            get => _selectedAvailableAddress;
            set
            {
                _selectedAvailableAddress = value;
                OnPropertyChanged();
                (BindAddressCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private ClientAddressDto _selectedBoundAddress;
        public ClientAddressDto SelectedBoundAddress
        {
            get => _selectedBoundAddress;
            set { _selectedBoundAddress = value; OnPropertyChanged(); }
        }

        public string NewAccountNumber { get; set; }
        public DateTime? OwnershipStartDate { get; set; } = DateTime.Today;
        public event Action? CloseRequested;
        public ICommand CloseWindowCommand { get; }

        public ICommand BindAddressCommand { get; }
        public ICommand UnbindAddressCommand { get; }

        public ClientAddressBindingViewModel(int clientId)
        {
            _clientId = clientId;
            CloseWindowCommand = new RelayCommand(_ => CloseRequested?.Invoke());

            BindAddressCommand = new RelayCommand(_ => BindAddress(), _ => SelectedAvailableAddress != null);
            UnbindAddressCommand = new RelayCommand(obj => UnbindAddress(obj), obj => obj is ClientAddressDto);
            LoadData();
        }

        private async Task SearchAddressesAsync()
        {
            if (string.IsNullOrWhiteSpace(AddressSearchText)) return;

            var searchTerms = AddressSearchText
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (searchTerms.Length == 0) return;

            await using var db = new HcsDbContext();

            var usedIds = db.ClientAddresses
                .Where(ca => ca.ClientId == _clientId)
                .Select(ca => ca.AddressId)
                .ToHashSet();

            var query = db.Addresses
                .Where(a => !a.IsDeleted && !usedIds.Contains(a.Id));

            foreach (var term in searchTerms)
            {
                var t = term; // для захвата в замыкание
                query = query.Where(a =>
                    EF.Functions.Like(a.City.ToLower(), $"%{t}%") ||
                    EF.Functions.Like(a.Street.ToLower(), $"%{t}%") ||
                    EF.Functions.Like(a.HouseNumber.ToLower(), $"%{t}%") ||
                    EF.Functions.Like((a.Building ?? "").ToLower(), $"%{t}%") ||
                    EF.Functions.Like((a.ApartmentNumber ?? "").ToLower(), $"%{t}%"));
            }

            var results = await query
                .OrderBy(a => a.City)
                .Take(100)
                .ToListAsync();

            SearchResults.Clear();
            foreach (var address in results)
            {
                SearchResults.Add(new ClientAddressDto
                {
                    AddressId = address.Id,
                    FullAddress = AddressHelpers.ToFullAddress(address)
                });
            }
        }


        private void LoadData()
        {
            using var db = new HcsDbContext();

            var bound = db.ClientAddresses
                .Where(ca => ca.ClientId == _clientId)
                .Include(ca => ca.Address)
                .ToList();

            var usedIds = bound.Select(ca => ca.AddressId).ToHashSet();

            var available = db.Addresses
                .Where(a => !a.IsDeleted && !usedIds.Contains(a.Id))
                .ToList();

            BoundAddresses.Clear();
            AvailableAddresses.Clear();

            foreach (var b in bound)
            {
                BoundAddresses.Add(new ClientAddressDto
                {
                    AddressId = b.AddressId,
                    FullAddress = AddressHelpers.ToFullAddress(b.Address),
                    PersonalAccountNumber = b.PersonalAccountNumber,
                    OwnershipStartDate = b.OwnershipStartDate
                });
            }

            foreach (var b in available)
                AvailableAddresses.Add(new ClientAddressDto
                {
                    AddressId = b.Id,
                    FullAddress = AddressHelpers.ToFullAddress(b)
                });

            OnPropertyChanged(nameof(BoundAddresses));
            OnPropertyChanged(nameof(AvailableAddresses));
        }

        private void BindAddress()
        {
            using var db = new HcsDbContext();

            var newBinding = new ClientAddress
            {
                ClientId = _clientId,
                AddressId = SelectedAvailableAddress.AddressId,
                OwnershipStartDate = OwnershipStartDate ?? DateTime.Today,
                OwnershipEndDate = null,
                PersonalAccountNumber = string.IsNullOrWhiteSpace(NewAccountNumber)
                    ? Guid.NewGuid().ToString()
                    : NewAccountNumber
            };

            db.ClientAddresses.Add(newBinding);
            db.SaveChanges();

            LoadData();
            NewAccountNumber = string.Empty;
            OwnershipStartDate = DateTime.Today;
            OnPropertyChanged(nameof(NewAccountNumber));
            OnPropertyChanged(nameof(OwnershipStartDate));
        }

        private void UnbindAddress(object? obj)
        {
            if (obj is not ClientAddressDto dto) return;

            var result = MessageBox.Show(
                $"Удалить привязку к адресу:\n{dto.FullAddress}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            using var db = new HcsDbContext();
            var binding = db.ClientAddresses.FirstOrDefault(ca =>
                ca.ClientId == _clientId && ca.AddressId == dto.AddressId);

            if (binding != null)
            {
                db.ClientAddresses.Remove(binding);
                db.SaveChanges();
                LoadData();
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
