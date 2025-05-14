using HCSSystem.ViewModels;
using System.Windows.Controls;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для ProfileFlipperControl.xaml
    /// </summary>
    public partial class ProfileFlipperControl : UserControl
    {
        public ProfileFlipperControl()
        {
            InitializeComponent();
            DataContext = new ProfileFlipperViewModel();
        }
    }
}
