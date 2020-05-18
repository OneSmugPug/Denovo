using System;
using System.Collections.Generic;
using System.Data;
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
        private string 
            employeeName, 
            filePath = @"C:\CSVDatabase",
            invNum,
            value,
            commPerc,
            commDue,
            total,
            date;
        private DataTable dt;
        private Window darkWindow;
        private static double windowMinHeight, windowMinWidth;
        private HwndSource hwndSource;
        private MainWindow owner;
        private readonly NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

        public Invoices()
        {
            InitializeComponent();

            windowMinHeight = MinHeight;
            windowMinWidth = MinWidth;
            nfi.NumberDecimalDigits = 2;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LblHeader.Content = "Invoices: " + employeeName;

            if (!File.Exists(filePath))
            {
                File.Create(filePath);

                dt = new DataTable();
                dt.Columns.Add(new DataColumn("Date", typeof(string)));
                dt.Columns.Add(new DataColumn("Invoice Number", typeof(string)));
                dt.Columns.Add(new DataColumn("Value (R)", typeof(string)));
                dt.Columns.Add(new DataColumn("Commission %", typeof(string)));
                dt.Columns.Add(new DataColumn("Commission Due (R)", typeof(string)));
                dt.Columns.Add(new DataColumn("Total (R)", typeof(string)));
                DataView dv = new DataView(dt);
                DGInvoice.ItemsSource = dv;
            }
            else
            {
                dt = new DataTable();
                dt.Columns.Add(new DataColumn("Date", typeof(string)));
                dt.Columns.Add(new DataColumn("Invoice Number", typeof(string)));
                dt.Columns.Add(new DataColumn("Value (R)", typeof(string)));
                dt.Columns.Add(new DataColumn("Commission %", typeof(string)));
                dt.Columns.Add(new DataColumn("Commission Due (R)", typeof(string)));
                dt.Columns.Add(new DataColumn("Total (R)", typeof(string)));
                DGInvoice.ItemsSource = ReadFile();
            }

            CalculateTotals();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            InvoiceAdd invAdd = new InvoiceAdd();
            invAdd.SetOwner(this);

            CreateDarkWindow();

            ApplyEffect(owner);
            darkWindow.Show();
            SetDarkWindowPos();

            if ((bool)invAdd.ShowDialog())
            {
                DataRow dr = dt.NewRow();
                dr[0] = date;
                dr[1] = invNum;
                dr[2] = value.Replace(',', ' ');
                dr[3] = commPerc.Replace(',', ' ');
                dr[4] = commDue.Replace(',', ' ');
                dr[5] = total.Replace(',', ' ');
                dt.Rows.Add(dr);

                DataView dv = new DataView(dt);
                DGInvoice.ItemsSource = dv;

                CalculateTotals();

                WriteCSV();
            }

            darkWindow.Hide();
            ClearEffect(owner);
        }

        private void BtnPayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentAdd payAdd = new PaymentAdd();
            payAdd.SetOwner(this);

            CreateDarkWindow();

            ApplyEffect(owner);
            darkWindow.Show();
            SetDarkWindowPos();

            if ((bool)payAdd.ShowDialog())
            {
                DataRow dr = dt.NewRow();
                dr[0] = date;
                dr[1] = "Payment";
                dr[2] = "-";
                dr[3] = "-";
                dr[4] = "-";
                dr[5] = total.Replace(',', ' ');
                dt.Rows.Add(dr);

                DataView dv = new DataView(dt);
                DGInvoice.ItemsSource = dv;

                CalculateTotals();

                WriteCSV();
            }

            darkWindow.Hide();
            ClearEffect(owner);
        }

        private void CalculateTotals()
        {
            decimal valueSum = 0m, commDueSum = 0m, totalSum = 0m;

            foreach(DataRow row in dt.Rows)
            {
                if (decimal.TryParse(row["Value (R)"].ToString().Replace('.', ','), out decimal valueResult))
                {
                    valueSum += valueResult;
                }

                if (decimal.TryParse(row["Commission Due (R)"].ToString().Replace('.', ','), out decimal commDueResult))
                {
                    commDueSum += commDueResult;
                }

                if (decimal.TryParse(row["Total (R)"].ToString().Replace('.', ','), out decimal totalResult))
                {
                    totalSum += totalResult;
                }
            }

            TxtValue.Text = valueSum.ToString("N2", nfi);
            TxtCommDue.Text = commDueSum.ToString("N2", nfi);
            TxtTotal.Text = totalSum.ToString("N2", nfi);
        }

        private void BtnOD_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(@"C:\CSVDatabase"))
                Process.Start(@"C:\CSVDatabase");
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        #region [Employees DataSource]
        public void WriteCSV()
        {
            DGInvoice.SelectAllCells();

            DGInvoice.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, DGInvoice);

            DGInvoice.UnselectAllCells();
            string result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);

            if (!File.Exists(filePath))
                File.AppendAllText(filePath, result, UnicodeEncoding.UTF8);
            else
            {
                File.Delete(filePath);
                File.AppendAllText(filePath, result, UnicodeEncoding.UTF8);
            }
        }

        public DataView ReadFile()
        {
            var lines = File.ReadAllLines(filePath);
            DataRow dr;

            foreach (string l in lines.Skip(1))
            {
                string[] split = l.Split(',');

                dr = dt.NewRow();

                dr[0] = split[0];
                dr[1] = split[1];
                dr[2] = split[2];
                dr[3] = split[3];
                dr[4] = split[4];
                dr[5] = split[5];

                dt.Rows.Add(dr);
            }

            return new DataView(dt);
        }
        #endregion

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

        public void SetEmployee(string name)
        {
            employeeName = name;
            filePath += @"\" + name + "_Invoices.csv";
        }

        public void SetOwner(MainWindow owner) => this.owner = owner;

        public void SetNewInvoice(string invNum, string value, string commPerc, string commDue, string total, DateTime date)
        {
            this.invNum = invNum;
            this.value = value;
            this.commPerc = commPerc;
            this.commDue = commDue;
            this.total = total;
            this.date = date.Date.ToShortDateString();
        }

        public void SetNewPayment(DateTime date, string total)
        {
            this.date = date.Date.ToShortDateString();
            this.total = total;
        }
    }
}
