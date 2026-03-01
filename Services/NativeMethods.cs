using System;
using System.Runtime.InteropServices;

namespace PotentialOverlay.Services
{
    public static class NativeMethods
    {
        // 1. 창 찾기 API
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        // 2. 창 위치 가져오기 API
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // 클릭 통과 및 투명 스타일 상수
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00000080;
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        public static RECT GetGameRect(string windowName)
        {
            IntPtr hwnd = FindWindow(null, windowName);
            if (hwnd != IntPtr.Zero)
            {
                GetWindowRect(hwnd, out RECT rect);
                return rect;
            }
            return new RECT { Left = 0, Top = 0, Right = 0, Bottom = 0 };
        }
    }
}