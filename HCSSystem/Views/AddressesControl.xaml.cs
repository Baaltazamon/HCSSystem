using HCSSystem.ViewModels;
using System.Windows.Controls;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для AddressesControl.xaml
    /// </summary>
    public partial class AddressesControl : UserControl
    {
        public AddressesControl()
        {
            InitializeComponent();
            DataContext = new AddressesViewModel();
        }
    }
}
