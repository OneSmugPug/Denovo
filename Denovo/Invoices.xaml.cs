using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Denovo
{
    /// <summary>
    /// Interaction logic for Invoices.xaml
    /// </summary>
    public partial class Invoices : UserControl
    {
        private string selectedEmpCode = string.Empty;
        private bool isFirst = true;
        private DataTable dt;
        private Window darkWindow;
        private static double windowMinHeight, windowMinWidth;
        private HwndSource hwndSource;
        private MainWindow owner;
        private readonly NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        private User USER;

        public Invoices()
        {
            InitializeComponent();

            windowMinHeight = MinHeight;
            windowMinWidth = MinWidth;
            nfi.NumberDecimalDigits = 2;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LblHeader.Content = "Invoices";

            if (USER.GetAccessLevel() == 1)
            {
                BtnPayment.Visibility = Visibility.Hidden;
                SpTotals.Children.RemoveAt(7);
                SpTotals.Children.RemoveAt(6);
                SpTotals.Children.RemoveAt(5);
                SpTotals.Children.RemoveAt(4);
                SpTotals.Width = double.NaN;
            }
            else if (USER.GetAccessLevel() == 2)
            {
                if (selectedEmpCode.Equals(string.Empty))
                {
                    BtnPayment.Visibility = Visibility.Hidden;
                    SpTotals.Children.RemoveAt(7);
                    SpTotals.Children.RemoveAt(6);
                    SpTotals.Width = double.NaN;
                }
            }

            LoadInvoices();

            CalculateTotals();
        }

        private void LoadInvoices()
        {
            try
            {
                using (var conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    var sql = string.Empty;

                    if (USER.GetAccessLevel() == 1)
                    {
                        sql = "SELECT Date, Client, [Invoice Number], [Bill Amount (R)], [Drawing Fee (%)], [DF Amount (R)], [Commission Due (R)], Finalized, Paid FROM Invoices WHERE "
                            + "Code = '" + USER.GetCode() + "'";
                    }
                    else if (USER.GetAccessLevel() == 2)
                    {
                        if (!selectedEmpCode.Equals(string.Empty))
                        {
                            sql = "SELECT Date, Client, [Invoice Number], [Bill Amount (R)], [Drawing Fee (%)], [DF Amount (R)], [Commission Due (R)], [Company Comm (R)], [Personal Comm (R)], "
                                + "Finalized, Paid FROM Invoices WHERE " + "Code = '" + selectedEmpCode + "'";
                        }
                        else
                        {
                            sql = "SELECT Date, Client, [Invoice Number], [Bill Amount (R)], [Drawing Fee (%)], [DF Amount (R)], [Commission Due (R)], [Company Comm (R)], [Personal Comm (R)], "
                                + "Finalized, Paid FROM Invoices";
                        }
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        dt = new DataTable();
                        da.Fill(dt);
                    }

                    DGInvoice.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            InvoiceAdd invAdd = new InvoiceAdd();
            invAdd.SetUser(USER);

            if (!selectedEmpCode.Equals(string.Empty))
                invAdd.SetSelectedEmployee(selectedEmpCode);

            CreateDarkWindow();

            ApplyEffect(owner);
            darkWindow.Show();
            SetDarkWindowPos();

            if ((bool)invAdd.ShowDialog())
            {
                LoadInvoices();
                isFirst = true;
                CalculateTotals();
            }

            darkWindow.Hide();
            ClearEffect(owner);
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = (DataRowView)DGInvoice.SelectedItem;

            CreateDarkWindow();

            ApplyEffect(owner);
            darkWindow.Show();
            SetDarkWindowPos();

            if (!row[2].ToString().StartsWith("P"))
            {
                InvoiceEdit invEdit = new InvoiceEdit();
                invEdit.SetUser(USER);
                invEdit.SetSelectedEmployee(selectedEmpCode);
                invEdit.SetSelectedInv(row[2].ToString());

                if ((bool)invEdit.ShowDialog())
                {
                    LoadInvoices();
                    isFirst = true;
                    CalculateTotals();
                }
            }
            else
            {
                PaymentEdit payEdit = new PaymentEdit();
                payEdit.SetSelectedPayment(row[2].ToString());

                if ((bool)payEdit.ShowDialog())
                {
                    LoadInvoices();
                    CalculateTotals();
                }
            }

            darkWindow.Hide();
            ClearEffect(owner);
        }

        private void BtnPayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentAdd payAdd = new PaymentAdd();
            payAdd.SetUser(USER);

            if (!selectedEmpCode.Equals(string.Empty))
                payAdd.SetSelectedEmployee(selectedEmpCode);

            CreateDarkWindow();

            ApplyEffect(owner);
            darkWindow.Show();
            SetDarkWindowPos();

            if ((bool)payAdd.ShowDialog())
            {
                LoadInvoices();
                isFirst = true;
                CalculateTotals();
            }

            darkWindow.Hide();
            ClearEffect(owner);
        }

        private void CalculateTotals()
        {
            decimal dfAmountSum = 0m, commDueSum = 0m, compCommSum = 0m, persCommSum = 0m;

            foreach (DataRow row in dt.Rows)
            {
                if (!isFirst)
                {
                    if (row["Paid"].ToString().Equals("No"))
                    {
                        if (decimal.TryParse(row["DF Amount (R)"].ToString().Replace(".", "").Replace(",", "").TrimStart('0'), out decimal valueResult))
                        {
                            valueResult /= 100;
                            dfAmountSum += valueResult;
                        }
                    }
                    else if (row["Paid"].ToString().Equals("Yes"))
                    {
                        if (decimal.TryParse(row["DF Amount (R)"].ToString().Replace(".", "").Replace(",", "").TrimStart('0'), out decimal valueResult))
                        {
                            valueResult /= 100;
                            dfAmountSum -= valueResult;
                        }
                    }
                }
                else
                {
                    if (row["Paid"].ToString().Equals("No"))
                    {
                        if (decimal.TryParse(row["DF Amount (R)"].ToString().Replace(".", "").Replace(",", "").TrimStart('0'), out decimal valueResult))
                        {
                            valueResult /= 100;
                            dfAmountSum += valueResult;
                        }
                    }
                    else if (row["Paid"].ToString().Equals("Yes"))
                    {
                        dfAmountSum -= 0.00m;
                    }

                    isFirst = false;
                }
                

                if (decimal.TryParse(row["Commission Due (R)"].ToString().Replace(".", "").Replace(",", "").TrimStart('0'), out decimal commDueResult))
                {
                    commDueResult /= 100;
                    commDueSum += commDueResult;
                }

                TxtDFAmount.Text = dfAmountSum.ToString("N2", nfi);
                TxtCommDue.Text = commDueSum.ToString("N2", nfi);

                if (USER.GetAccessLevel() == 2)
                {

                    if (decimal.TryParse(row["Company Comm (R)"].ToString().Replace(".", "").Replace(",", "").TrimStart('0'), out decimal compCommResult))
                    {
                        compCommResult /= 100;
                        compCommSum += compCommResult;
                    }

                    if (!selectedEmpCode.Equals(string.Empty))
                    {
                        if (decimal.TryParse(row["Personal Comm (R)"].ToString().Replace(".", "").Replace(",", "").TrimStart('0'), out decimal persCommResult))
                        {
                            persCommResult /= 100;
                            persCommSum += persCommResult;
                        }

                        TxtPersonalComm.Text = persCommSum.ToString("N2", nfi);
                    }

                    TxtCompComm.Text = compCommSum.ToString("N2", nfi);
                }
            }
        }

        private void DGInvoice_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd/MM/yyyy";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        #region [Window Move & Resize]
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0046:
                    {
                        var pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
                        if ((pos.flags & (int)(SWP.NOMOVE)) != 0)
                            return IntPtr.Zero;

                        var wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                        if (wnd == null)
                            return IntPtr.Zero;

                        bool changedPos = false;
                        if (pos.cx < windowMinWidth) { pos.cx = (int)windowMinWidth; changedPos = true; }
                        if (pos.cy < windowMinHeight) { pos.cy = (int)windowMinHeight; changedPos = true; }
                        if (!changedPos)
                            return IntPtr.Zero;

                        Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                        break;
                    }
            }
            return (IntPtr)0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        enum SWP : uint
        {
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOSENDCHANGING = 0x0400,
        }
        #endregion

        #region [Dark Background Effect]
        private void CreateDarkWindow()
        {
            darkWindow = new Window()
            {
                Background = Brushes.Black,
                Opacity = 0.4,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false
            };

            darkWindow.SourceInitialized += (s, ev) =>
            {
                IntPtr handle = (new WindowInteropHelper(darkWindow)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
        }

        private void SetDarkWindowPos()
        {
            if (owner.WindowState == WindowState.Maximized)
            {
                darkWindow.Top = owner.Top;
                darkWindow.Left = owner.Left;
                darkWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                darkWindow.WindowState = WindowState.Normal;

                darkWindow.Width = owner.Width;
                darkWindow.Height = owner.Height;
                darkWindow.Top = owner.Top;
                darkWindow.Left = owner.Left;
            }
        }
        #endregion

        #region [Blur Effect]
        private void ApplyEffect(Window win)
        {
            var objBlur = new BlurEffect { Radius = 4 };
            win.Effect = objBlur;
        }

        private void ClearEffect(Window win)
        {
            win.Effect = null;
        }
        #endregion

        public void SetOwner(MainWindow owner) => this.owner = owner;

        public void SetUser(User USER) => this.USER = USER;

        public void SetSelectedEmployee(string selectedEmpCode) => this.selectedEmpCode = selectedEmpCode;
    }
}
