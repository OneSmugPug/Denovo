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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Denovo
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private MainWindow owner;
        private DataTable dt;

        public Login()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TxtUsername.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!TxtUsername.Text.Equals(string.Empty) && !PbPassword.Password.Equals(string.Empty))
            {
                try
                {
                    using (SqlConnection conn = DBUtils.GetDBConnection())
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter("SELECT Code, Name, [Access Level] FROM Employees WHERE Password = '" + PbPassword.Password + "' AND Code = '" + TxtUsername.Text + "'", conn))
                        {
                            dt = new DataTable();
                            da.Fill(dt);
                        }

                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];

                            User USER = new User(row["Code"].ToString(), int.Parse(row["Access Level"].ToString().Split('-')[0].Trim()), row["Name"].ToString());

                            owner.LoginSuccessful(USER);
                        }
                        else LblError.Content = "User does not exist. Contact administrator for assistance";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (TxtUsername.Text.Equals(string.Empty) && PbPassword.Password.Equals(string.Empty))
                LblError.Content = "Username & Password can not be empty!";
            else if (TxtUsername.Text.Equals(string.Empty))
                LblError.Content = "Username can not be empty!";
            else if (PbPassword.Password.Equals(string.Empty))
                LblError.Content = "Password can not be empty!";
        }

        private void TxtUsername_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            LblError.Content = string.Empty;
        }

        private void PbPassword_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            LblError.Content = string.Empty;
        }

        public void SetOwner(MainWindow owner) => this.owner = owner;
    }
}
