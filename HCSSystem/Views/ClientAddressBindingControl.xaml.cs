using HCSSystem.ViewModels;
using System.Windows;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для ClientAddressBindingControl.xaml
    /// </summary>
    public partial class ClientAddressBindingWindow : Window
    {
        public ClientAddressBindingWindow(int clientId)
        {
            InitializeComponent();
            DataContext = new ClientAddressBindingViewModel(clientId);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
