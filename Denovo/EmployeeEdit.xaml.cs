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
    public partial class EmployeeEdit : Window
    {
        private string code;

        public EmployeeEdit()
        {
            InitializeComponent();
        }

        #region [Window Load]
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtEmployeeCode.Text = code;
            TxtEmployee.Focus();

            CbAccessLevel.Items.Add("1 - Employee");
            CbAccessLevel.Items.Add("2 - Administrator");

            LoadEmployee();
        }

        private void LoadEmployee()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    using (var da = new SqlDataAdapter("SELECT Name, Password, [Access Level] FROM Employees WHERE Code = '" + code + "'", conn))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                TxtEmployee.Text = row["Name"].ToString();
                TxtPassword.Text = row["Password"].ToString();

                CbAccessLevel.SelectedItem = row["Access Level"].ToString();
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

                            using (var cmd = new SqlCommand("UPDATE Employees SET Name = @EmpName, Password = @Password, [Access Level] = @AccessLevel WHERE Code = '" + code + "'", conn))
                            {
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

        public void SetSelectedEmployee(string code) => this.code = code;
    }
}
