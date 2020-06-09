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
    public partial class ClientEdit : Window
    {
        private string code;

        public ClientEdit()
        {
            InitializeComponent();
        }

        #region [Window Load]
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtClientCode.Text = code;

            TxtCompanyName.Focus();

            LoadClient();
        }

        private void LoadClient()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    using (var da = new SqlDataAdapter("SELECT Company, Name, Address FROM Clients WHERE Code = '" + code + "'", conn))
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

                TxtCompanyName.Text = row["Company"].ToString();
                TxtClientName.Text = row["Name"].ToString();

                TxtUnitSuite.Text = row["Address"].ToString().Split(';')[0].Trim();
                TxtAddress.Text = row["Address"].ToString().Split(';')[1].Trim();
                TxtCity.Text = row["Address"].ToString().Split(';')[2].Trim();
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

                            using (var cmd = new SqlCommand("UPDATE Clients SET Company = @CompanyName, Name = @Name, Address = @Address WHERE Code = '" + code + "'", conn))
                            {
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

        public void SetSelectedClient(string code) => this.code = code;
    }
}
