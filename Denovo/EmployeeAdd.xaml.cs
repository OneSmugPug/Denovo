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

namespace Denovo
{
    /// <summary>
    /// Interaction logic for EmployeeAdd.xaml
    /// </summary>
    public partial class EmployeeAdd : Window
    {
        private Employees owner;

        public EmployeeAdd()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtEmployee.Focus();
        }

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            if (!TxtEmployee.Text.Equals(string.Empty))
            {
                owner.SetNewEmployeeName(TxtEmployee.Text);
                DialogResult = true;
            }
        }

        private void TxtEmployee_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtEmployee.Text.Equals(string.Empty))
            {
                ImgEmployeeError.Visibility = Visibility.Visible;
                TxtEmployee.ToolTip = "Client code can not be empty.";
                TxtEmployee.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));
            }
        }

        private void TxtEmployee_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ImgEmployeeError.Visibility = Visibility.Hidden;
            TxtEmployee.ClearValue(ToolTipProperty);
            TxtEmployee.ClearValue(BorderBrushProperty);
        }

        public void SetOwner(Employees owner) => this.owner = owner;
    }
}
