using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Denovo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static double windowMinHeight, windowMinWidth;
        private HwndSource hwndSource;
        private User USER;
        private bool isLoggedIn = false;

        #region [Windows Load]
        public MainWindow()
        {
            InitializeComponent();

            LblWelcome.Visibility = Visibility.Hidden;
            LblName.Visibility = Visibility.Hidden;

            windowMinHeight = MinHeight;
            windowMinWidth = MinWidth;

            //Used to allow screen to resize and perform normally with WindowStyle set to None
            SourceInitialized += (s, e) =>
            {
                var handle = new WindowInteropHelper(this).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BtnInvoices.Visibility = Visibility.Hidden;
            BtnExpenses.Visibility = Visibility.Hidden;
            BtnEmployees.Visibility = Visibility.Hidden;
            BtnClients.Visibility = Visibility.Hidden;

            WindowState = WindowState.Maximized;
            BtnLogin.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
        #endregion

        #region [Navigation Tab Button Click Events]
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            ClearButtonBackgrounds();
            BtnLogin.Background = new SolidColorBrush(Color.FromRgb(95, 95, 95));
            Workspace.Children.Clear();

            if (isLoggedIn)
            {
                TbLogin.Text = "Login";
                ImgLogin.Source = Application.Current.TryFindResource("login") as DrawingImage;

                BtnInvoices.Visibility = Visibility.Hidden;
                BtnExpenses.Visibility = Visibility.Hidden;
                BtnEmployees.Visibility = Visibility.Hidden;
                BtnClients.Visibility = Visibility.Hidden;

                LblWelcome.Visibility = Visibility.Hidden;
                LblName.Visibility = Visibility.Hidden;
            }

            Login login = new Login
            {
                Height = double.NaN,
                Width = double.NaN
            };
            login.SetOwner(this);

            Workspace.Children.Add(login);
        }

        private void BtnInvoices_Click(object sender, RoutedEventArgs e)
        {
            ClearButtonBackgrounds();
            BtnInvoices.Background = new SolidColorBrush(Color.FromRgb(95, 95, 95));
            Workspace.Children.Clear();

            Invoices inv = new Invoices
            {
                Height = double.NaN,
                Width = double.NaN
            };
            inv.SetUser(USER);
            inv.SetOwner(this);

            Workspace.Children.Add(inv);
        }

        private void BtnExpenses_Click(object sender, RoutedEventArgs e)
        {
            ClearButtonBackgrounds();
            BtnExpenses.Background = new SolidColorBrush(Color.FromRgb(95, 95, 95));
            Workspace.Children.Clear();

            Expenses exp = new Expenses
            {
                Height = double.NaN,
                Width = double.NaN
            };
            exp.SetOwner(this);

            Workspace.Children.Add(exp);
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e)
        {
            ClearButtonBackgrounds();
            BtnEmployees.Background = new SolidColorBrush(Color.FromRgb(95, 95, 95));
            Workspace.Children.Clear();

            Employees emp = new Employees
            {
                Height = double.NaN,
                Width = double.NaN,
            };
            emp.SetOwner(this);

            Workspace.Children.Add(emp);
        }

        private void BtnClients_Click(object sender, RoutedEventArgs e)
        {
            ClearButtonBackgrounds();
            BtnClients.Background = new SolidColorBrush(Color.FromRgb(95, 95, 95));
            Workspace.Children.Clear();

            Clients cli = new Clients
            {
                Height = double.NaN,
                Width = double.NaN,
            };
            cli.SetOwner(this);

            Workspace.Children.Add(cli);
        }

        private void ClearButtonBackgrounds()
        {
            BtnLogin.ClearValue(BackgroundProperty);
            BtnInvoices.ClearValue(BackgroundProperty);
            BtnExpenses.ClearValue(BackgroundProperty);
            BtnEmployees.ClearValue(BackgroundProperty);
            BtnClients.ClearValue(BackgroundProperty);
        }
        #endregion

        public void EmployeeSelected(string selectedEmpCode)
        {
            Workspace.Children.Clear();

            Invoices inv = new Invoices
            {
                Height = double.NaN,
                Width = double.NaN
            };
            inv.SetOwner(this);
            inv.SetSelectedEmployee(selectedEmpCode);
            inv.SetUser(USER);

            Workspace.Children.Add(inv);
        }

        public void LoginSuccessful(User USER)
        {
            this.USER = USER;

            TbLogin.Text = "Logout";
            ImgLogin.Source = Application.Current.TryFindResource("logout") as DrawingImage;
            isLoggedIn = true;

            if (USER.GetAccessLevel() == 1)
            {
                BtnInvoices.Visibility = Visibility.Visible;
            }
            else if (USER.GetAccessLevel() == 2)
            {
                BtnInvoices.Visibility = Visibility.Visible;
                BtnExpenses.Visibility = Visibility.Visible;
                BtnEmployees.Visibility = Visibility.Visible;
                BtnClients.Visibility = Visibility.Visible;
            }

            LblName.Content = USER.GetName();
            LblWelcome.Visibility = Visibility.Visible;
            LblName.Visibility = Visibility.Visible;

            BtnInvoices.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        #region [Window Move & Resize]
        private void BtnWindowMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            BtnWindowMaximize.Visibility = Visibility.Hidden;
            BtnWindowRestore.Visibility = Visibility.Visible;
        }

        private void BtnWindowRestore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            BtnWindowRestore.Visibility = Visibility.Hidden;
            BtnWindowMaximize.Visibility = Visibility.Visible;
        }

        private void BtnWindowMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (WindowState == WindowState.Maximized)
                {
                    var point = PointToScreen(e.MouseDevice.GetPosition(this));

                    Left = point.X - (RestoreBounds.Width / 2);

                    Top = point.Y - (((FrameworkElement)sender).ActualHeight / 2);
                    WindowState = WindowState.Normal;
                }

                DragMove();
            }
        }

        private void Main_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                BtnWindowMaximize.Visibility = Visibility.Hidden;
                BtnWindowRestore.Visibility = Visibility.Visible;
            }
            else if (WindowState == WindowState.Normal)
            {
                BtnWindowRestore.Visibility = Visibility.Hidden;
                BtnWindowMaximize.Visibility = Visibility.Visible;
            }
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
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

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            var MONITOR_DEFAULTTONEAREST = 0x00000002;
            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);

                var rcWorkArea = monitorInfo.rcWork;
                var rcMonitorArea = monitorInfo.rcMonitor;

                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
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

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

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
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
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

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

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

        private void InitializeWindowSource(object sender, EventArgs e) => hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;

        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
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

        private void BtnWindowClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
