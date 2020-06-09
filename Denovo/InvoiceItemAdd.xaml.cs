using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
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
    /// Interaction logic for InvoiceAdd.xaml
    /// </summary>
    public partial class InvoiceItemAdd : Window
    {
        private readonly NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        private decimal amount1 = 0.00m, amount2 = 0.00m, amount3 = 0.00m;
        private InvoiceDocument invDoc;

        public InvoiceItemAdd()
        {
            InitializeComponent();
            nfi.NumberDecimalDigits = 2;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtRef.Focus();
            TxtAmount1.Text = "0.00";
            TxtAmount2.Text = "0.00";
            TxtAmount3.Text = "0.00";

            TxtAmount1.SelectionStart = TxtAmount1.Text.Length;
            TxtAmount2.SelectionStart = TxtAmount2.Text.Length;
            TxtAmount3.SelectionStart = TxtAmount3.Text.Length;
        }

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            invDoc.Reference = TxtRef.Text.Trim();
            invDoc.Description1 = TxtDesc1.Text.Trim();
            invDoc.Description2 = TxtDesc2.Text.Trim();
            invDoc.Description3 = TxtDesc3.Text.Trim();

            if (amount1 != 0.00m)
                invDoc.Amount1 = "R " + amount1.ToString("N2", nfi);

            if (amount2 != 0.00m)
                invDoc.Amount2 = "R " + amount2.ToString("N2", nfi);

            if (amount3 != 0.00m)
                invDoc.Amount3 = "R " + amount3.ToString("N2", nfi);

            DialogResult = true;
        }

        private void TxtAmount1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TxtAmount1.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
            {
                result /= 100;
                amount1 = result;
                TxtAmount1.TextChanged -= TxtAmount1_TextChanged;
                TxtAmount1.Text = result.ToString("N2", nfi);
                TxtAmount1.TextChanged += TxtAmount1_TextChanged;
                TxtAmount1.Select(TxtAmount1.Text.Length, 0);
            }
        }

        private void TxtAmount1_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtAmount1.Text.Equals(string.Empty) || TxtAmount1.Text.Equals("0"))
                TxtAmount1.Text = "0.00";
        }

        private void TxtAmount2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TxtAmount2.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
            {
                result /= 100;
                amount2 = result;
                TxtAmount2.TextChanged -= TxtAmount2_TextChanged;
                TxtAmount2.Text = result.ToString("N2", nfi);
                TxtAmount2.TextChanged += TxtAmount2_TextChanged;
                TxtAmount2.Select(TxtAmount2.Text.Length, 0);
            }
        }

        private void TxtAmount2_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtAmount2.Text.Equals(string.Empty) || TxtAmount2.Text.Equals("0"))
                TxtAmount2.Text = "0.00";
        }

        private void TxtAmount3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TxtAmount3.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
            {
                result /= 100;
                amount3 = result;
                TxtAmount3.TextChanged -= TxtAmount3_TextChanged;
                TxtAmount3.Text = result.ToString("N2", nfi);
                TxtAmount3.TextChanged += TxtAmount3_TextChanged;
                TxtAmount3.Select(TxtAmount3.Text.Length, 0);
            }
        }

        private void TxtAmount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Delete)
                e.Handled = false;
            else e.Handled = true;
        }

        private void TxtAmount3_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtAmount3.Text.Equals(string.Empty) || TxtAmount3.Text.Equals("0"))
                TxtAmount3.Text = "0.00";
        }

        public void SetInvoiceDoc(InvoiceDocument invDoc) => this.invDoc = invDoc;
    }
}
