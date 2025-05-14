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
using System.Windows.Shapes;
using HCSSystem.Helpers;
using HCSSystem.ViewModels;

namespace HCSSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для AddEditAddressDialog.xaml
    /// </summary>
    public partial class AddEditAddressDialog : Window
    {
        public AddEditAddressDialog()
        {
            InitializeComponent();
            DataContext = new AddEditAddressViewModel();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !BaseMethods.IsTextAllowed(e.Text);
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            BaseMethods.HandleTextBoxPasting(sender, e);
        }
    }
}
