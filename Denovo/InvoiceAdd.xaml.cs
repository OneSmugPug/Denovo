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
    public partial class InvoiceAdd : Window
    {
        private readonly NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        private decimal commPerc = 0, value = 0;
        private bool commPercError = false, isOwner = false;
        private Invoices owner;

        public InvoiceAdd()
        {
            InitializeComponent();
            nfi.NumberDecimalDigits = 2;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtInvoiceNumber.Focus();
            TxtValue.Text = "0.00";
            TxtCommDue.Text = "0.00";
            TxtCommPerc.Text = "11.00%";
            commPerc = 0.11m;
            TxtValue.SelectionStart = TxtValue.Text.Length;

            DtpDate.SelectedDate = DateTime.Now;
        }

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            if (!TxtInvoiceNumber.Text.Equals(string.Empty) && !commPercError)
            {
                decimal total = 0;

                if (decimal.TryParse(TxtCommDue.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
                {
                    result /= 100;

                    if (!isOwner)
                        total = result * 0.4m;
                    else total = result * 0.6m;
                }

                owner.SetNewInvoice(TxtInvoiceNumber.Text, TxtValue.Text, commPerc.ToString("N2", nfi), TxtCommDue.Text, total.ToString("N2", nfi), DtpDate.SelectedDate.Value);
                DialogResult = true;
            }
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

                TxtCommDue.Text = (result * commPerc).ToString("N2", nfi);
            }
        }

        private void TxtInvoiceNumber_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtInvoiceNumber.Text.Equals(string.Empty))
            {
                ImgInvNumError.Visibility = Visibility.Visible;
                TxtInvoiceNumber.ToolTip = "Client code can not be empty.";
                TxtInvoiceNumber.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));
            }
        }

        private void TxtInvoiceNumber_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ImgInvNumError.Visibility = Visibility.Hidden;
            TxtInvoiceNumber.ClearValue(ToolTipProperty);
            TxtInvoiceNumber.ClearValue(BorderBrushProperty);
        }

        private void TxtCommPerc_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (decimal.TryParse(TxtCommPerc.Text.Replace('.', ','), out decimal result))
            {
                result /= 100;
                commPerc = result;
                if (result >= 0 && result <= 1)
                    TxtCommPerc.Text = result.ToString("p", nfi);
                else
                {
                    ImgCommPercError.Visibility = Visibility.Visible;
                    TxtCommPerc.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));
                    TxtCommPerc.ToolTip = "Input must be between 0 and 100.";
                    commPercError = true;
                }

                TxtCommDue.Text = (value * commPerc).ToString("N2", nfi);
            }
            else if (TxtCommPerc.Text.Equals(string.Empty))
            {
                TxtCommPerc.Text = "0%";
            }
            else
            {
                ImgCommPercError.Visibility = Visibility.Visible;
                TxtCommPerc.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));
                TxtCommPerc.ToolTip = "Input must be between 0 and 100.";
                commPercError = true;
            }         
        }

        private void TxtCommPerc_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (commPercError)
            {
                ImgCommPercError.Visibility = Visibility.Hidden;
                TxtCommPerc.ClearValue(ToolTipProperty);
                TxtCommPerc.ClearValue(BorderBrushProperty);
                commPercError = false;
            }

            TxtCommPerc.Text = TxtCommPerc.Text.Replace("%", "");
            TxtCommPerc.SelectAll();
        }

        public void SetOwner(Invoices owner) => this.owner = owner;
    }
}
