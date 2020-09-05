using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DBD_perk
{
    public class WindowFinder
    {        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);


        public Rect GetDbdRect()
        {
            Process[] processes = Process.GetProcessesByName("DeadByDaylight-Win64-Shipping");
            Process process = processes[0];
            IntPtr ptr = process.MainWindowHandle;
            Rect DbdRect = new Rect();
            GetWindowRect(ptr, ref DbdRect);
            return DbdRect;
        }
    }


}
