using HCSSystem.ViewModels;
using System.Windows.Controls;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для EmployeesView.xaml
    /// </summary>
    public partial class EmployeesView : UserControl
    {
        public EmployeesView()
        {
            InitializeComponent();
            DataContext = new EmployeesViewModel();
        }
    }
}
