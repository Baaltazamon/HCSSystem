using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels
{
    public class AddResidentDialogViewModel : INotifyPropertyChanged
    {
        private readonly int _addressId;

        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; } = DateTime.Today;
        public DateTime? RegistrationDate { get; set; } = DateTime.Today;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? CloseRequested;

        public AddResidentDialogViewModel(int addressId)
        {
            _addressId = addressId;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(FirstName) ||
                BirthDate == null || RegistrationDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new HcsDbContext();

            db.Residents.Add(new Resident
            {
                AddressId = _addressId,
                LastName = LastName,
                FirstName = FirstName,
                MiddleName = MiddleName,
                BirthDate = BirthDate.Value,
                RegistrationDate = RegistrationDate.Value,
                EndRegistrationDate = null,
                IsDeleted = false
            });

            db.SaveChanges();
            CloseRequested?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
