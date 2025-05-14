using HCSSystem.ViewModels;
using System.Windows.Controls;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для PaymentsView.xaml
    /// </summary>
    public partial class PaymentsView : UserControl
    {
        public PaymentsView()
        {
            InitializeComponent();
            DataContext = new PaymentsViewModel();
        }
    }
}
