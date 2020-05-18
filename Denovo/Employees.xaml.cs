using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for Employees.xaml
    /// </summary>
    public partial class Employees : UserControl
    {
        private string filePath = @"C:\CSVDatabase\Employees.csv", newEmployeeName;
        private DataTable dt;
        private static double windowMinHeight, windowMinWidth;
        private HwndSource hwndSource;
        private MainWindow owner;
        private Window darkWindow;

        public Employees()
        {
            InitializeComponent();

            windowMinHeight = MinHeight;
            windowMinWidth = MinWidth;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath);

                dt = new DataTable();
                dt.Columns.Add(new DataColumn("Employee Name", typeof(string)));
                DataView dv = new DataView(dt);
                DGEmployees.ItemsSource = dv;
            }
            else
            {
                dt = new DataTable();
                dt.Columns.Add(new DataColumn("Employee Name", typeof(string)));
                DGEmployees.ItemsSource = ReadFile();
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            EmployeeAdd empAdd = new EmployeeAdd();
            empAdd.SetOwner(this);

            CreateDarkWindow();

            ApplyEffect(owner);
            darkWindow.Show();
            SetDarkWindowPos();

            if ((bool)empAdd.ShowDialog())
            {
                DataRow dr = dt.NewRow();
                dr[0] = newEmployeeName;
                dt.Rows.Add(dr);

                DataView dv = new DataView(dt);
                DGEmployees.ItemsSource = dv;

                WriteCSV();
            }

            darkWindow.Hide();
            ClearEffect(owner);
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = (DataRowView)DGEmployees.SelectedItem;
            owner.EmployeeSelected(row[0].ToString());
        }

        #region [Employees DataSource]
        public void WriteCSV()
        {
            DGEmployees.SelectAllCells();

            DGEmployees.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, DGEmployees);

            DGEmployees.UnselectAllCells();

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
                string[] split = l.Split(';');

                dr = dt.NewRow();
                dr[0] = split[0];

                dt.Rows.Add(dr);
            }

            return new DataView(dt);
        }
        #endregion

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

        public void SetNewEmployeeName(string employeeName)
        {
            newEmployeeName = employeeName;
        }
    }
}
