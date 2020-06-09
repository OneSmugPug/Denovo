using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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
    public partial class Clients : UserControl
    {
        private DataTable dt;
        private static double windowMinHeight, windowMinWidth;
        private HwndSource hwndSource;
        private MainWindow owner;
        private Window darkWindow;

        public Clients()
        {
            InitializeComponent();

            windowMinHeight = MinHeight;
            windowMinWidth = MinWidth;
        }

        #region [Window Loaded]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClients();
        }

        private void LoadClients()
        {
            try
            {
                using (SqlConnection conn = DBUtils.GetDBConnection())
                {
                    conn.Open();

                    using (var da = new SqlDataAdapter("SELECT * FROM Clients", conn))
                    {
                        dt = new DataTable();
                        da.Fill(dt);
                    }

                    DGClients.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region [Add/Edit]
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClientAdd cliAdd = new ClientAdd();

            CreateDarkWindow();

            ApplyEffect(owner);
            darkWindow.Show();
            SetDarkWindowPos();

            if ((bool)cliAdd.ShowDialog())
            {
                LoadClients();
            }

            darkWindow.Hide();
            ClearEffect(owner);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DGClients.SelectedItem != null)
            {
                var row = (DataRowView)DGClients.SelectedItem;

                ClientEdit cliEdit = new ClientEdit();
                cliEdit.SetSelectedClient(row[0].ToString());

                CreateDarkWindow();

                ApplyEffect(owner);
                darkWindow.Show();
                SetDarkWindowPos();

                if ((bool)cliEdit.ShowDialog())
                {
                    LoadClients();
                }

                darkWindow.Hide();
                ClearEffect(owner);
            }
            else MessageBox.Show("Please select a client.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        public void SetOwner(MainWindow owner) => this.owner = owner;
    }
}
