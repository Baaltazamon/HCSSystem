using HCSSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для PaymentsChartCard.xaml
    /// </summary>
    public partial class PaymentsChartCard : UserControl
    {
        public PaymentsChartCard()
        {
            InitializeComponent();
            DataContext = new PaymentsChartCardViewModel();
        }
    }
}
