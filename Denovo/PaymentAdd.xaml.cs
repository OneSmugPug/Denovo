using System;
using System.Collections.Generic;
using System.Globalization;
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
    public partial class PaymentAdd : Window
    {
        private readonly NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        private decimal value = 0;
        private bool isOwner = false;
        private Invoices owner;

        public PaymentAdd()
        {
            InitializeComponent();
            nfi.NumberDecimalDigits = 2;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtValue.Focus();
            TxtValue.Text = "0.00";
            TxtValue.SelectionStart = TxtValue.Text.Length;

            DtpDate.SelectedDate = DateTime.Now;
        }

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            owner.SetNewPayment(DtpDate.SelectedDate.Value, "-" + TxtValue.Text);
            DialogResult = true;
        }

        private void TxtValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TxtValue.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
            {
                result /= 100;
                value = result;
                TxtValue.TextChanged -= TxtValue_TextChanged;
                TxtValue.Text = result.ToString("N2", nfi);
                TxtValue.TextChanged += TxtValue_TextChanged;
                TxtValue.Select(TxtValue.Text.Length, 0);
            }
        }

        public void SetOwner(Invoices owner) => this.owner = owner;
    }
}
