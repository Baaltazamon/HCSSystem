using HCSSystem.Data;
using HCSSystem.Data.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using HCSSystem.Helpers;
using HCSSystem.Views;

namespace HCSSystem.ViewModels
{
    public class ClientAddressesViewModel : INotifyPropertyChanged
    {
        private readonly int _clientId;

        public ObservableCollection<ClientAddress> ClientAddresses { get; set; } = new();
        public ICommand OpenBindAddressWindowCommand { get; }
        public ClientAddressesViewModel(int clientId)
        {
            _clientId = clientId;
            OpenBindAddressWindowCommand = new RelayCommand(_ => OpenBindingWindow());
            LoadAddresses();
        }

        private void LoadAddresses()
        {
            using var db = new HcsDbContext();
            var addresses = db.ClientAddresses
                .Where(ca => ca.ClientId == _clientId).Include(c=> c.Address)
                .ToList();

            ClientAddresses.Clear();
            foreach (var address in addresses)
                ClientAddresses.Add(address);
        }

        private void OpenBindingWindow()
        {
            var window = new ClientAddressBindingWindow(_clientId);
            window.ShowDialog();
            
            LoadAddresses();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}