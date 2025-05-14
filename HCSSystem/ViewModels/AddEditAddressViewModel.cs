using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using HCSSystem.Data.Entities;
using HCSSystem.Data;
using HCSSystem.Helpers;

namespace HCSSystem.ViewModels
{
    public class AddEditAddressViewModel : INotifyPropertyChanged
    {
        public event Action CloseRequested;

        public bool IsEditMode { get; set; } = false;

        public int? Id { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string Building { get; set; }
        public string ApartmentNumber { get; set; }
        public decimal PropertyArea { get; set; } = 0;
        public bool IsResidential { get; set; } = true;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEditAddressViewModel()
        {
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        }

        public void LoadFromAddress(Address address)
        {
            Id = address.Id;
            City = address.City;
            Street = address.Street;
            HouseNumber = address.HouseNumber;
            Building = address.Building;
            ApartmentNumber = address.ApartmentNumber;
            PropertyArea = address.PropertyArea;
            IsResidential = address.IsResidential;
        }

        private void Save()
        {
            using var db = new HcsDbContext();

            if (string.IsNullOrWhiteSpace(City) ||
                string.IsNullOrWhiteSpace(HouseNumber) ||
                string.IsNullOrWhiteSpace(ApartmentNumber))
            {
                MessageBox.Show("Поля 'Город', 'Дом' и 'Квартира' обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsEditMode && Id.HasValue)
            {
                var address = db.Addresses.FirstOrDefault(a => a.Id == Id);
                if (address == null) return;

                address.City = City;
                address.Street = Street;
                address.HouseNumber = HouseNumber;
                address.Building = Building;
                address.ApartmentNumber = ApartmentNumber;
                address.PropertyArea = PropertyArea;
                address.IsResidential = IsResidential;
            }
            else
            {
                var newAddress = new Address
                {
                    City = City,
                    Street = Street,
                    HouseNumber = HouseNumber,
                    Building = Building,
                    ApartmentNumber = ApartmentNumber,
                    PropertyArea = PropertyArea,
                    IsResidential = IsResidential,
                    IsDeleted = false
                };
                db.Addresses.Add(newAddress);
            }

            db.SaveChanges();
            CloseRequested?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
