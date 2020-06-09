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
using Word = Microsoft.Office.Interop.Word;
using Aspose.Words;

namespace Denovo
{
    /// <summary>
    /// Interaction logic for InvoiceAdd.xaml
    /// </summary>
    public partial class InvoiceAdd : Window
    {
        private readonly NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        private decimal drawingFee = 0, billAmount = 0, dfAmount = 0, commDue = 0, companyComm = 0, personalComm = 0;
        private DataTable dt;
        private string selectedEmpCode = string.Empty, company = string.Empty;
        private string[] address;
        private bool isAddressDone = false, isCityDone = false;
        private User USER;
        InvoiceDocument invDoc;
        private readonly Dictionary<string, string> addresses = new Dictionary<string, string>();
        private readonly Dictionary<string, string> companyNames = new Dictionary<string, string>();

        public InvoiceAdd()
        {
            InitializeComponent();
            nfi.NumberDecimalDigits = 2;
        }

        #region [Window Load]
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CbClient.Focus();
            TxtBillAmount.Text = "0.00";
            TxtCommDue.Text = "0.00";
            TxtDFAmount.Text = "0.00";
            TxtDrawingFee.Text = "11.00";

            if (decimal.TryParse(TxtDrawingFee.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
            {
                result /= 10000;
                drawingFee = result;
            }

            TxtBillAmount.SelectionStart = TxtBillAmount.Text.Length;
            TxtDrawingFee.SelectionStart = TxtDrawingFee.Text.Length;

            DtpDate.SelectedDate = DateTime.Now;
            CbFinalized.IsChecked = false;

            LoadClients();

            GetInvoiceNumber();
        }

        private void LoadClients()
        {
            try
            {
                using (var conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    using (var da = new SqlDataAdapter("SELECT * FROM Clients", conn))
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
                {
                    CbClient.Items.Add(row["Code"].ToString() + " - " + row["Name"].ToString());
                    addresses.Add(row["Code"].ToString(), row["Address"].ToString());
                    companyNames.Add(row["Code"].ToString(), row["Company"].ToString());
                }

                CbClient.SelectedIndex = 0;
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

                        if (!curInvNum.Equals(string.Empty) && curInvNum.StartsWith("D"))
                        {
                            int curInvNumDigit = int.Parse(curInvNum.Remove(0, 3));
                            if (curInvNumDigit > newInvNumDigit)
                                newInvNumDigit = curInvNumDigit;
                        }
                    }

                    newInvNumDigit++;
                    TxtInvoiceNumber.Text = "DEN" + newInvNumDigit.ToString("00000");
                }
                else TxtInvoiceNumber.Text = "DEN00001";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            if (!TxtInvoiceNumber.Text.Equals(string.Empty))
            {
                StringBuilder sb = new StringBuilder().Append("Are you sure you want to continue?");

                if (MessageBox.Show(sb.ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (USER.GetAccessLevel() == 1 || (USER.GetAccessLevel() == 2 && selectedEmpCode.Equals(string.Empty)))
                    {
                        try
                        {
                            using (var conn = DBUtils.GetDBConnection())
                            {
                                conn.Open();

                                using (var cmd = new SqlCommand("INSERT INTO Invoices VALUES (@Code, @Date, @Client, @InvNum, @BillAmount, @DrawFee, @DFAmount, @CommDue, @CompComm, @PersonalComm, @Finalized, @Paid)", conn))
                                {
                                    cmd.Parameters.AddWithValue("@Code", USER.GetCode());
                                    cmd.Parameters.AddWithValue("@Date", DtpDate.SelectedDate.Value.Date);
                                    cmd.Parameters.AddWithValue("@Client", CbClient.SelectedItem.ToString().Split('-')[0].Trim());
                                    cmd.Parameters.AddWithValue("@InvNum", TxtInvoiceNumber.Text.Trim());
                                    cmd.Parameters.AddWithValue("@BillAmount", billAmount);
                                    cmd.Parameters.AddWithValue("@DrawFee", drawingFee);
                                    cmd.Parameters.AddWithValue("@DFAmount", dfAmount);
                                    cmd.Parameters.AddWithValue("@CommDue", commDue);
                                    cmd.Parameters.AddWithValue("@CompComm", companyComm);
                                    cmd.Parameters.AddWithValue("@PersonalComm", personalComm);
                                    cmd.Parameters.AddWithValue("@Paid", "No");

                                    if (!(bool)CbFinalized.IsChecked)
                                        cmd.Parameters.AddWithValue("@Finalized", "No");
                                    else cmd.Parameters.AddWithValue("@Finalized", "Yes");

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
                    else if (USER.GetAccessLevel() == 2 && !selectedEmpCode.Equals(string.Empty))
                    {
                        try
                        {
                            using (var conn = DBUtils.GetDBConnection())
                            {
                                conn.Open();

                                using (var cmd = new SqlCommand("INSERT INTO Invoices VALUES (@Code, @Date, @Client, @InvNum, @BillAmount, @DrawFee, @DFAmount, @CommDue, @CompComm, @PersonalComm, @Finalized, @Paid)", conn))
                                {
                                    cmd.Parameters.AddWithValue("@Code", selectedEmpCode);
                                    cmd.Parameters.AddWithValue("@Date", DtpDate.SelectedDate.Value.Date);
                                    cmd.Parameters.AddWithValue("@Client", CbClient.SelectedItem.ToString().Split('-')[0].Trim());
                                    cmd.Parameters.AddWithValue("@InvNum", TxtInvoiceNumber.Text.Trim());
                                    cmd.Parameters.AddWithValue("@BillAmount", billAmount);
                                    cmd.Parameters.AddWithValue("@DrawFee", drawingFee);
                                    cmd.Parameters.AddWithValue("@DFAmount", dfAmount);
                                    cmd.Parameters.AddWithValue("@CommDue", commDue);
                                    cmd.Parameters.AddWithValue("@CompComm", companyComm);
                                    cmd.Parameters.AddWithValue("@PersonalComm", personalComm);

                                    if (!(bool)CbFinalized.IsChecked)
                                        cmd.Parameters.AddWithValue("@Finalized", "No");
                                    else cmd.Parameters.AddWithValue("@Finalized", "Yes");

                                    if (!(bool)CbPaid.IsChecked)
                                        cmd.Parameters.AddWithValue("@Paid", "No");
                                    else cmd.Parameters.AddWithValue("@Paid", "Yes");

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
        }

        private void TxtBillAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TxtBillAmount.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
            {
                result /= 100;
                billAmount = result;
                TxtBillAmount.TextChanged -= TxtBillAmount_TextChanged;
                TxtBillAmount.Text = result.ToString("N2", nfi);
                TxtBillAmount.TextChanged += TxtBillAmount_TextChanged;
                TxtBillAmount.Select(TxtBillAmount.Text.Length, 0);

                dfAmount = result * drawingFee;
                TxtDFAmount.Text = dfAmount.ToString("N2", nfi);
                CalculateCommissionDue();
            }
        }

        private void TxtBillAmount_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TxtBillAmount.Text.Equals(string.Empty) || TxtBillAmount.Text.Equals("0"))
                TxtBillAmount.Text = "0.00";
        }

        private void TxtDrawingFee_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (decimal.TryParse(TxtDrawingFee.Text.Replace(",", "").Replace(".", "").Replace("%", "").TrimStart('0'), out decimal result))
            {
                result /= 10000;
                drawingFee = result;
                dfAmount = billAmount * drawingFee;
                TxtDFAmount.Text = dfAmount.ToString("N2", nfi);
                CalculateCommissionDue();
            }
            else if (TxtDrawingFee.Text.Equals(string.Empty))
                TxtDrawingFee.Text = "11.00";
            else if (TxtDrawingFee.Text.Equals("0"))
                TxtDrawingFee.Text = "0.00";     
        }

        private void TxtBillAmount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Delete)
                e.Handled = false;
            else e.Handled = true;
        }

        private void TxtDrawingFee_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Delete)
                e.Handled = false;
            else e.Handled = true;
        }

        private void TxtDrawingFee_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TxtDrawingFee.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
            {
                result /= 100;
                TxtDrawingFee.TextChanged -= TxtDrawingFee_TextChanged;
                TxtDrawingFee.Text = result.ToString("N2", nfi);
                TxtDrawingFee.TextChanged += TxtDrawingFee_TextChanged;
                TxtDrawingFee.Select(TxtDrawingFee.Text.Length, 0);
            }
        }

        private void CbClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addresses.TryGetValue(CbClient.SelectedItem.ToString().Split('-')[0].Trim(), out string addressResult))
            {
                address = addressResult.Split(';');
            }

            if (companyNames.TryGetValue(CbClient.SelectedItem.ToString().Split('-')[0].Trim(), out string companyResult))
            {
                company = companyResult;
            }
        }

        private void CbFinalized_Checked(object sender, RoutedEventArgs e)
        {
            invDoc = new InvoiceDocument();

            InvoiceItemAdd invItemAdd = new InvoiceItemAdd();
            invItemAdd.SetInvoiceDoc(invDoc);

            string path = Directory.GetCurrentDirectory();
            path = System.IO.Path.Combine(path, "Denovo_Template.docx");

            if ((bool)invItemAdd.ShowDialog())
                CreateWordDocment(path);
        }

        private void CreateWordDocment(object fileName)
        {
            //Set missing value - Used to represent a missing value when calling methods through interop
            object missing = System.Reflection.Missing.Value;

            //Setup Word apllication class
            Word.Application wordApp = new Word.Application();

            //Setup the word document
            Word.Document wordDoc = null;

            try
            {
                //Check to see if the file exists
                if (File.Exists((string)fileName))
                {
                    //Open the word document
                    wordDoc = wordApp.Documents.Open(fileName, missing, missing);

                    //Activate the document
                    wordApp.Selection.Find.ClearFormatting();
                    wordApp.Selection.Find.Replacement.ClearFormatting();

                    //Find place holders and replace them with values
                    wordApp.Selection.Find.Execute("<invnum>", missing, missing, missing, missing, missing, missing, missing, missing, TxtInvoiceNumber.Text.Trim(), 2);
                    wordApp.Selection.Find.Execute("<companyname>", missing, missing, missing, missing, missing, missing, missing, missing, company.ToUpper(), 2);

                    if (!address[0].Equals(string.Empty))
                        wordApp.Selection.Find.Execute("<unitsuite>", missing, missing, missing, missing, missing, missing, missing, missing, address[0].Trim().ToUpper(), 2);
                    else if (address[0].Equals(string.Empty))
                    {
                        if (!address[1].Equals(string.Empty))
                        {
                            wordApp.Selection.Find.Execute("<unitsuite>", missing, missing, missing, missing, missing, missing, missing, missing, address[1].Trim().ToUpper(), 2);
                            isAddressDone = true;
                        }
                        else
                        {
                            wordApp.Selection.Find.Execute("<unitsuite>", missing, missing, missing, missing, missing, missing, missing, missing, address[2].Trim().ToUpper(), 2);
                            isCityDone = true;
                        }
                    }

                    if (!isAddressDone)
                    {
                        if (!address[1].Equals(string.Empty))
                            wordApp.Selection.Find.Execute("<addressline>", missing, missing, missing, missing, missing, missing, missing, missing, address[1].Trim().ToUpper(), 2);
                        else if (!isCityDone)
                        {
                            wordApp.Selection.Find.Execute("<addressline>", missing, missing, missing, missing, missing, missing, missing, missing, address[2].Trim().ToUpper(), 2);
                            isCityDone = true;
                        }
                        else wordApp.Selection.Find.Execute("<addressline>", missing, missing, missing, missing, missing, missing, missing, missing, string.Empty, 2);
                    }
                    else
                    {
                        wordApp.Selection.Find.Execute("<addressline>", missing, missing, missing, missing, missing, missing, missing, missing, address[2].Trim().ToUpper(), 2);
                        isCityDone = true;
                    }

                    if (!isCityDone)
                        wordApp.Selection.Find.Execute("<city>", missing, missing, missing, missing, missing, missing, missing, missing, address[2].Trim().ToUpper(), 2);
                    else wordApp.Selection.Find.Execute("<city>", missing, missing, missing, missing, missing, missing, missing, missing, string.Empty, 2);

                    wordApp.Selection.Find.Execute("<reference>", missing, missing, missing, missing, missing, missing, missing, missing, invDoc.Reference, 2);
                    wordApp.Selection.Find.Execute("<date>", missing, missing, missing, missing, missing, missing, missing, missing, DtpDate.SelectedDate.Value.ToShortDateString(), 2);
                    wordApp.Selection.Find.Execute("<total>", missing, missing, missing, missing, missing, missing, missing, missing, "R " + dfAmount.ToString("N2", nfi), 2);

                    if (!invDoc.Description1.Equals(string.Empty))
                        wordApp.Selection.Find.Execute("<desc1>", missing, missing, missing, missing, missing, missing, missing, missing, invDoc.Description1, 2);
                    else wordApp.Selection.Find.Execute("<desc1>", missing, missing, missing, missing, missing, missing, missing, missing, string.Empty, 2);

                    if (!invDoc.Description2.Equals(string.Empty))
                        wordApp.Selection.Find.Execute("<desc2>", missing, missing, missing, missing, missing, missing, missing, missing, invDoc.Description2, 2);
                    else wordApp.Selection.Find.Execute("<desc2>", missing, missing, missing, missing, missing, missing, missing, missing, string.Empty, 2);

                    if (!invDoc.Description3.Equals(string.Empty))
                        wordApp.Selection.Find.Execute("<desc3>", missing, missing, missing, missing, missing, missing, missing, missing, invDoc.Description3, 2);
                    else wordApp.Selection.Find.Execute("<desc3>", missing, missing, missing, missing, missing, missing, missing, missing, string.Empty, 2);

                    if (!invDoc.Amount1.Equals(string.Empty))
                        wordApp.Selection.Find.Execute("<amount1>", missing, missing, missing, missing, missing, missing, missing, missing, invDoc.Amount1, 2);
                    else wordApp.Selection.Find.Execute("<amount1>", missing, missing, missing, missing, missing, missing, missing, missing, string.Empty, 2);

                    if (!invDoc.Amount2.Equals(string.Empty))
                        wordApp.Selection.Find.Execute("<amount2>", missing, missing, missing, missing, missing, missing, missing, missing, invDoc.Amount2, 2);
                    else wordApp.Selection.Find.Execute("<amount2>", missing, missing, missing, missing, missing, missing, missing, missing, string.Empty, 2);

                    if (!invDoc.Amount3.Equals(string.Empty))
                        wordApp.Selection.Find.Execute("<amount3>", missing, missing, missing, missing, missing, missing, missing, missing, invDoc.Amount3, 2);
                    else wordApp.Selection.Find.Execute("<amount3>", missing, missing, missing, missing, missing, missing, missing, missing, string.Empty, 2);
                }
                else
                {
                    MessageBox.Show("File does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Directory.Exists(@"C:\Invoices"))
                    Directory.CreateDirectory(@"C:\Invoices");

                //Save the document
                object SaveAsFile = (object)@"C:\Invoices\" + TxtInvoiceNumber.Text.Trim() + ".docx";
                wordDoc.SaveAs2(SaveAsFile, ref missing, ref missing, ref missing);

                //Document pdfDoc = (Document)wordDoc;
                //pdfDoc.Save(@"C:\Invoices\" + TxtInvoiceNumber.Text.Trim() + ".pdf");

                wordDoc.Close(ref missing, ref missing, ref missing);
                wordApp.Quit(ref missing, ref missing, ref missing);
                MessageBox.Show("File successfully created at " + @"C:\Invoices\" + TxtInvoiceNumber.Text.Trim() + ".docx");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                wordDoc.Close(ref missing, ref missing, ref missing);
                wordApp.Quit(ref missing, ref missing, ref missing);
            }
            
        }

        private void CalculateCommissionDue()
        {
            if (decimal.TryParse(TxtDFAmount.Text.Replace(",", "").Replace(".", "").TrimStart('0'), out decimal result))
            {
                result /= 100;
                commDue = result * 0.4m;
                companyComm = result * 0.6m;
                personalComm = result * 0.3m;
                TxtCommDue.Text = commDue.ToString("N2", nfi);
            }
        }

        public void SetSelectedEmployee(string selectedEmpCode) => this.selectedEmpCode = selectedEmpCode;

        public void SetUser(User USER) => this.USER = USER; 
    }
}
