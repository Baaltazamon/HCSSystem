using HCSSystem.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            MainContentArea.Content = new MainDashboardView();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new ClientsView();
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new EmployeesView();
        }

        private void AddressesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new AddressesControl();
        }
        private void ServicesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new ServicesView
            {
                DataContext = new ServicesViewModel()
            };
        }

        private void RatesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new RatesView
            {
                DataContext = new RatesViewModel()
            };
        }

        private void MetersButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new MetersView
            {
                DataContext = new MetersViewModel()
            };
        }

        private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None; 
            }
        }

        private void PaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new PaymentsView();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new MainDashboardView();
        }
    }
}