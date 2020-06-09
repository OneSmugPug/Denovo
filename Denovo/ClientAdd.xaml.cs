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
    public partial class ClientAdd : Window
    {
        public ClientAdd()
        {
            InitializeComponent();
        }

        #region [Window Load]
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtCompanyName.Focus();

            LoadClientCode();
        }

        private void LoadClientCode()
        {
            DataTable dt = new DataTable();
            int newCodeNum = 0;

            try
            {
                using (SqlConnection conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    using (var da = new SqlDataAdapter("SELECT Code FROM Clients", conn))
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
                    TxtClientCode.Text = "CLT" + newCodeNum.ToString("00000");
                }
                else TxtClientCode.Text = "CLT00001";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            if (!TxtClientName.Text.Equals(string.Empty))
            {
                StringBuilder sb = new StringBuilder().Append("Are you sure you want to continue?");

                if (MessageBox.Show(sb.ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DBUtils.GetDBConnection())
                        {
                            conn.Open();

                            using (var cmd = new SqlCommand("INSERT INTO Clients VALUES (@Code, @CompanyName, @Name, @Address)", conn))
                            {
                                cmd.Parameters.AddWithValue("@Code", TxtClientCode.Text.Trim());
                                cmd.Parameters.AddWithValue("@CompanyName", TxtCompanyName.Text.Trim());
                                cmd.Parameters.AddWithValue("@Name", TxtClientName.Text.Trim());
                                cmd.Parameters.AddWithValue("@Address", TxtUnitSuite.Text.Trim() + ";" + TxtAddress.Text.Trim() + ";" + TxtCity.Text.Trim());

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

        #region [ClientName KeyboardFocus]
        private void TxtClientName_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtClientName.Text.Equals(string.Empty))
            {
                ImgClientNameError.Visibility = Visibility.Visible;
                TxtClientName.ToolTip = "Client name can not be empty.";
                TxtClientName.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));
            }
        }

        private void TxtClientName_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ImgClientNameError.Visibility = Visibility.Hidden;
            TxtClientName.ClearValue(ToolTipProperty);
            TxtClientName.ClearValue(BorderBrushProperty);
        }
        #endregion
    }
}
