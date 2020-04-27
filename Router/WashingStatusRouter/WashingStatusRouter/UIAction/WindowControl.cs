using System;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace WashingStatusRouter
{
    class FormFunction
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;
        const int HTCAPTION = 0x0002;
        public static void MoveForm(object sender, MouseButtonEventArgs e, Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            ReleaseCapture();
            SendMessage(hwnd, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

    }
}
