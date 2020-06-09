using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        public EmployeeAdd()
        {
            InitializeComponent();
        }

        #region [Window Load]
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtEmployee.Focus();

            CbAccessLevel.Items.Add("1 - Employee");
            CbAccessLevel.Items.Add("2 - Administrator");
            CbAccessLevel.SelectedIndex = 0;

            LoadEmployeeCode();
        }

        private void LoadEmployeeCode()
        {
            DataTable dt = new DataTable();
            int newCodeNum = 0;

            try
            {
                using (SqlConnection conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    using (var da = new SqlDataAdapter("SELECT Code FROM Employees", conn))
                    {
                        da.Fill(dt);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string curCode = row["Code"].ToString().Trim();
                        int curCodeNum = int.Parse(curCode.Remove(0, 3));
                        if (curCodeNum > newCodeNum)
                            newCodeNum = curCodeNum;
                    }

                    newCodeNum++;
                    TxtEmployeeCode.Text = "EMP" + newCodeNum.ToString("000");
                }
                else TxtEmployeeCode.Text = "EMP001";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        } 
        #endregion

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            if (!TxtEmployee.Text.Equals(string.Empty) && !TxtPassword.Text.Equals(string.Empty))
            {
                StringBuilder sb = new StringBuilder().Append("Are you sure you want to continue?");

                if (MessageBox.Show(sb.ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DBUtils.GetDBConnection())
                        {
                            conn.Open();

                            using (var cmd = new SqlCommand("INSERT INTO Employees VALUES (@Code, @EmpName, @Password, @AccessLevel)", conn))
                            {
                                cmd.Parameters.AddWithValue("@Code", TxtEmployeeCode.Text.Trim());
                                cmd.Parameters.AddWithValue("@EmpName", TxtEmployee.Text.Trim());
                                cmd.Parameters.AddWithValue("@Password", TxtPassword.Text.Trim());
                                cmd.Parameters.AddWithValue("@AccessLevel", CbAccessLevel.SelectedItem.ToString());

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
        }

        #region [KeyboardFocus]
        private void TxtEmployee_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtEmployee.Text.Equals(string.Empty))
            {
                ImgEmployeeError.Visibility = Visibility.Visible;
                TxtEmployee.ToolTip = "Employee name can not be empty.";
                TxtEmployee.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));
            }
        }

        private void TxtEmployee_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ImgEmployeeError.Visibility = Visibility.Hidden;
            TxtEmployee.ClearValue(ToolTipProperty);
            TxtEmployee.ClearValue(BorderBrushProperty);
        }

        private void TxtPassword_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtPassword.Text.Equals(string.Empty))
            {
                ImgPasswordError.Visibility = Visibility.Visible;
                TxtPassword.ToolTip = "Password can not be empty.";
                TxtPassword.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));
            }
        }

        private void TxtPassword_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ImgPasswordError.Visibility = Visibility.Hidden;
            TxtPassword.ClearValue(ToolTipProperty);
            TxtPassword.ClearValue(BorderBrushProperty);
        } 
        #endregion
    }
}
