using HCSSystem.ViewModels;
using System.Windows;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для AddressPeopleWindow.xaml
    /// </summary>
    public partial class AddressPeopleWindow : Window
    {
        public AddressPeopleWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RegisterResident_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddressPeopleViewModel vm)
            {
                var dialog = new AddResidentDialog(vm.AddressId);
                if (dialog.DataContext is AddResidentDialogViewModel addVm)
                {
                    addVm.CloseRequested += () =>
                    {
                        dialog.Close();
                        vm.RefreshPeople();
                    };
                }

                dialog.ShowDialog();
            }
        }
    }
}
