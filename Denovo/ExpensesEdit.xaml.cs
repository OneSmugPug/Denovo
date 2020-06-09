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
    public partial class ExpensesEdit : Window
    {
        private readonly NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        private decimal amount = 0;
        private DataTable dt;
        private string selectedCode = string.Empty;

        public ExpensesEdit()
        {
            InitializeComponent();
            nfi.NumberDecimalDigits = 2;
        }

        #region [Window Load]
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtDescription.Focus();

            TxtAmount.SelectionStart = TxtAmount.Text.Length;

            LoadEmployees();

            LoadExpense();
        }

        private void LoadEmployees()
        {
            try
            {
                using (var conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    using (var da = new SqlDataAdapter("SELECT Code, Name FROM Employees", conn))
                    {
                        dt = new DataTable();
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                    CbEmployee.Items.Add(row["Code"].ToString() + " - " + row["Name"].ToString());

                CbEmployee.SelectedIndex = 0;
            }
        }

        private void LoadExpense()
        {
            try
            {
                using (var conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    var sql = "SELECT Date, Description, [Amount (R)], [Employee Code], [Employee] FROM Expenses WHERE " +
                        " Code = '" + selectedCode + "'";

                    using (var da = new SqlDataAdapter(sql, conn))
                    {
                        dt = new DataTable();
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];

                        foreach (string item in CbEmployee.Items)
                        {
                            if (row["Employee Code"].ToString().Equals(item.Split('-')[0].Trim()))
                                CbEmployee.SelectedItem = item;
                        }

                        DtpDate.SelectedDate = DateTime.Parse(row["Date"].ToString());
                        TxtDescription.Text = row["Description"].ToString();

                        if (decimal.TryParse(row["Amount (R)"].ToString().Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
                        {
                            result /= 100;
                            amount = result;
                            TxtAmount.Text = amount.ToString("N2", nfi);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

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

                        using (var cmd = new SqlCommand("UPDATE Expenses SET Date = @Date, Description = @Description, [Amount (R)] = @Amount, [Employee Code] = @EmpCode, Employee = @Employee "
                            + "WHERE Code = @Code", conn))
                        {
                            cmd.Parameters.AddWithValue("@Code", selectedCode);
                            cmd.Parameters.AddWithValue("@Date", DtpDate.SelectedDate.Value.Date);
                            cmd.Parameters.AddWithValue("@Description", TxtDescription.Text.Trim());
                            cmd.Parameters.AddWithValue("@Amount", amount);
                            cmd.Parameters.AddWithValue("@EmpCode", CbEmployee.SelectedItem.ToString().Split('-')[0].Trim());
                            cmd.Parameters.AddWithValue("@Employee", CbEmployee.SelectedItem.ToString().Split('-')[1].Trim());

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

        private void TxtAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TxtAmount.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
            {
                result /= 100;
                amount = result;
                TxtAmount.TextChanged -= TxtAmount_TextChanged;
                TxtAmount.Text = result.ToString("N2", nfi);
                TxtAmount.TextChanged += TxtAmount_TextChanged;
                TxtAmount.Select(TxtAmount.Text.Length, 0);
            }
        }

        private void TxtAmount_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtAmount.Text.Equals(string.Empty) || TxtAmount.Text.Equals("0"))
                TxtAmount.Text = "0.00";
        }

        public void SetSelectedExpense(string selectedCode) => this.selectedCode = selectedCode;

        private void TxtAmount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Delete)
                e.Handled = false;
            else e.Handled = true;
        }
    }
}
