using HCSSystem.ViewModels;
using System.Windows;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для AddResidentDialog.xaml
    /// </summary>
    public partial class AddResidentDialog : Window
    {
        public AddResidentDialog(int addressId)
        {
            InitializeComponent();
            DataContext = new AddResidentDialogViewModel(addressId);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
