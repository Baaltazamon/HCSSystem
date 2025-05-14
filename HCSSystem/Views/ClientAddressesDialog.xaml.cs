
using System.Windows;
using HCSSystem.ViewModels;

namespace HCSSystem.Views
{
    public partial class ClientAddressesDialog : Window
    {
        public ClientAddressesDialog(int clientId)
        {
            InitializeComponent();
            DataContext = new ClientAddressesViewModel(clientId);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}