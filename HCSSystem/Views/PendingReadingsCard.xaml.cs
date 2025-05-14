using HCSSystem.ViewModels;
using System.Windows.Controls;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для PendingReadingsCard.xaml
    /// </summary>
    public partial class PendingReadingsCard : UserControl
    {
        public PendingReadingsCard()
        {
            InitializeComponent();
            DataContext = new PendingReadingsCardViewModel();
        }
    }
}
