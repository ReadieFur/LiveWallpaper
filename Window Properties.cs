using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace LiveWallpaper
{
    class Window_Properties
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static void setBottom(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        public static void disableActivation(Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(Point lpPoint);
        public static bool IsWindowVisible(Window window)
        {
            WindowInteropHelper win = new WindowInteropHelper(window);
            int x = (int)(window.Left + (window.Width / 2));
            int y = (int)(window.Top + (window.Height / 2));
            Point p = new Point(x, y);
            return win.Handle == WindowFromPoint(p);
        }
    }
}
