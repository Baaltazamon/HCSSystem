using HCSSystem.Data.Entities;
using HCSSystem.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Threading.Tasks;
using HCSSystem.Helpers;
using System.Windows;
using HCSSystem.Views;
using Microsoft.EntityFrameworkCore;
using HCSSystem.ViewModels.Models;

namespace HCSSystem.ViewModels
{
    public class AddressesViewModel : INotifyPropertyChanged
    {
        private string _searchQuery;
        private bool _isLoading;
        private int _currentPage = 1;
        private const int PageSize = 1000;
        private int _totalPages = 1;

        public ObservableCollection<Address> Addresses { get; set; } = new();
        public ICollectionView FilteredAddresses { get; set; }
        

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                CurrentPage = 1;
                OnPropertyChanged();
                _ = LoadAddressesAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (value != _currentPage && value >= 1 && value <= TotalPages)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    _ = LoadAddressesAsync();
                    RaisePaginationCommands();
                }
            }
        }

        private int _selectedFilterIndex = 0;
        public int SelectedFilterIndex
        {
            get => _selectedFilterIndex;
            set
            {
                if (_selectedFilterIndex != value)
                {
                    _selectedFilterIndex = value;
                    OnPropertyChanged();
                    _ = LoadAddressesAsync();
                    RaisePaginationCommands();
                }
            }
        }


        public int TotalPages
        {
            get => _totalPages;
            set { _totalPages = value; OnPropertyChanged(); }
        }

        public ICommand AddAddressCommand { get; set; }
        public ICommand EditAddressCommand { get; set; }
        public ICommand DeleteAddressCommand { get; set; }
        public ICommand NextPageCommand { get; set; }
        public ICommand PreviousPageCommand { get; set; }
        public ICommand OpenPeopleCommand { get; set; }
        public ICommand ExportReportCommand { get; }
        public AddressesViewModel()
        {
            FilteredAddresses = CollectionViewSource.GetDefaultView(Addresses);

            AddAddressCommand = new RelayCommand(_ => OpenAddressDialog(null));
            EditAddressCommand = new RelayCommand(obj => OpenAddressDialog(obj as Address), obj => obj is Address);
            DeleteAddressCommand = new RelayCommand(DeleteAddress, obj => obj is Address);
            OpenPeopleCommand = new RelayCommand(address => OpenPeopleWindow(address as Address));

            NextPageCommand = new RelayCommand(_ => CurrentPage++, _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(_ => CurrentPage--, _ => CurrentPage > 1);
            ExportReportCommand = new RelayCommand(_ => ReportService.GenerateAddressReport());

            _ = LoadAddressesAsync();
        }
        private void RaisePaginationCommands()
        {
            (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        public async Task LoadAddressesAsync()
        {
            try
            {
                IsLoading = true;
                Addresses.Clear();

                await using var db = new HcsDbContext();

                var query = db.Addresses.Where(a => !a.IsDeleted);

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    query = query.Where(a =>
                        EF.Functions.Like(a.City, $"%{SearchQuery}%") ||
                        EF.Functions.Like(a.Street ?? "", $"%{SearchQuery}%") ||
                        EF.Functions.Like(a.HouseNumber, $"%{SearchQuery}%") ||
                        EF.Functions.Like(a.Building ?? "", $"%{SearchQuery}%") ||
                        EF.Functions.Like(a.ApartmentNumber, $"%{SearchQuery}%") ||
                        a.PropertyArea.ToString().Contains(SearchQuery));
                }
                if (SelectedFilterIndex == 1)
                {
                    // Только с жильцами
                    query = query.Where(a => db.ClientAddresses.Any(ca => ca.AddressId == a.Id));
                }
                else if (SelectedFilterIndex == 2)
                {
                    // Только без жильцов
                    query = query.Where(a => !db.ClientAddresses.Any(ca => ca.AddressId == a.Id));
                }
                // Ну а по умолчанию хуярим все 198к адресов на 1к на страницу
                var totalCount = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

                var pageItems = await 
                    query.OrderBy(a => a.Id)
                         .Skip((CurrentPage - 1) * PageSize)
                         .Take(PageSize)
                         .ToListAsync();

                foreach (var addr in pageItems)
                    Addresses.Add(addr);

                FilteredAddresses.Refresh();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка при загрузке адресов: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenPeopleWindow(Address? address)
        {
            if (address == null) return;

            var window = new AddressPeopleWindow
            {
                DataContext = new AddressPeopleViewModel(address.Id)
            };

            window.ShowDialog();
        }

        private void EditAddress(object obj)
        {
            if (obj is not Address addr) return;

            var dialog = new AddEditAddressDialog();
            if (dialog.DataContext is AddEditAddressViewModel vm)
            {
                vm.IsEditMode = true;
                vm.LoadFromAddress(addr);
                vm.CloseRequested += () =>
                {
                    dialog.Close();
                    _ = LoadAddressesAsync();
                };
            }
            dialog.ShowDialog();
        }

        private void OpenAddressDialog(Address address)
        {
            var dialog = new AddEditAddressDialog();
            if (dialog.DataContext is AddEditAddressViewModel vm)
            {
                if (address != null)
                {
                    vm.IsEditMode = true;
                    vm.LoadFromAddress(address);
                }

                vm.CloseRequested += () =>
                {
                    dialog.Close();
                    _ = LoadAddressesAsync();
                };
            }

            dialog.ShowDialog();
        }


        private void DeleteAddress(object obj)
        {
            if (obj is not Address addr) return;

            if (MessageBox.Show("Удалить выбранный адрес?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using var db = new HcsDbContext();
                var entity = db.Addresses.FirstOrDefault(a => a.Id == addr.Id);
                if (entity != null)
                {
                    entity.IsDeleted = true;
                    db.SaveChanges();
                    _ = LoadAddressesAsync();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
