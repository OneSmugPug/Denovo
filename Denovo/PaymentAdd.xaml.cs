using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        private User USER;
        private string selectedEmpCode = string.Empty, invNum = string.Empty;
        private DataTable dt;

        public PaymentAdd()
        {
            InitializeComponent();
            nfi.NumberDecimalDigits = 2;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CbEmployees.Focus();
            TxtValue.Text = "0.00";
            TxtValue.SelectionStart = TxtValue.Text.Length;

            DtpDate.SelectedDate = DateTime.Now;

            LoadEmployees();

            GetInvoiceNumber();
        }

        private void LoadEmployees()
        {
            try
            {
                using (var conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    var sql = "SELECT Code, Name FROM Employees";

                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        dt = new DataTable();
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            CbEmployees.Items.Add(row["Code"].ToString() + " - " + row["Name"].ToString());
                        }

                        CbEmployees.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetInvoiceNumber()
        {
            dt = new DataTable();
            int newInvNumDigit = 0;

            try
            {
                using (SqlConnection conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    using (var da = new SqlDataAdapter("SELECT [Invoice Number] FROM Invoices", conn))
                    {
                        da.Fill(dt);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string curInvNum = row["Invoice Number"].ToString().Trim();

                        if (!curInvNum.Equals(string.Empty) && curInvNum.StartsWith("P"))
                        {
                            int curInvNumDigit = int.Parse(curInvNum.Remove(0, 3));
                            if (curInvNumDigit > newInvNumDigit)
                                newInvNumDigit = curInvNumDigit;
                        }
                    }

                    newInvNumDigit++;
                    invNum = "PAY" + newInvNumDigit.ToString("00000");
                }
                else invNum = "PAY00001";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder().Append("Are you sure you want to continue?");

            if (MessageBox.Show(sb.ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var conn = DBUtils.GetDBConnection())
                    {
                        conn.Open();

                        using (var cmd = new SqlCommand("INSERT INTO Invoices (Code, Date, [Invoice Number], [Commission Due (R)]) VALUES (@Code, @Date, @InvNum, @CommDue)", conn))
                        {
                            value *= -1;

                            cmd.Parameters.AddWithValue("@Code", CbEmployees.SelectedItem.ToString().Split('-')[0].Trim());
                            cmd.Parameters.AddWithValue("@Date", DtpDate.SelectedDate.Value.Date);
                            cmd.Parameters.AddWithValue("@InvNum", invNum);
                            cmd.Parameters.AddWithValue("@CommDue", value);

                            cmd.ExecuteNonQuery();

                            DialogResult = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
            }
        }

        public void SetUser(User USER) => this.USER = USER;

        public void SetSelectedEmployee(string selectedEmpCode) => this.selectedEmpCode = selectedEmpCode;

        private void TxtValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Delete)
                e.Handled = false;
            else e.Handled = true;
        }
    }
}
