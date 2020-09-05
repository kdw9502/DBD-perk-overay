using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace DBD_perk
{
    public partial class MainWindow : Window
    {
        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
            public int Height => Bottom - Top;
            public int Width => Right - Left;
        }
        DispatcherTimer repositionTimer = new DispatcherTimer();
        Process dbdProcess;
        IntPtr dbdHandle;
        Rect dbdRect;


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public MainWindow()
        {
            InitializeComponent();
            PerkInfoLoader.load();
            StartTimer();
            
            Topmost = true;
        }

        private void StartTimer()
        {
            UpdateDbdOveray(null, null);

            repositionTimer.Interval = TimeSpan.FromMilliseconds(0.5);
            repositionTimer.Tick += new EventHandler(UpdateDbdOveray); 
            repositionTimer.Start();            
        }

        private void UpdateDbdOveray(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("DeadByDaylight-Win64-Shipping");

            try
            {
                dbdProcess = processes[0];
                dbdHandle = dbdProcess.MainWindowHandle;
                dbdRect = new Rect();
                GetWindowRect(dbdHandle, ref dbdRect);
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show("실행중인 데바데를 찾지 못했습니다. 프로그램을 종료합니다.");
                this.Close();
            }
            SetRect(dbdRect);
            //GetPerkImage();
        }

        private void SetRect(Rect rect)
        {
            Top = rect.Top;
            Left = rect.Left;
            Height = rect.Height;
            Width = rect.Width;
            
        }



        public void GetPerkImage()
        {
            int imageWidth = dbdRect.Width/8;
            int imageHeight = imageWidth;

            var bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                
                graphics.CopyFromScreen(dbdRect.Left + dbdRect.Width - imageWidth, dbdRect.Top + dbdRect.Height -imageHeight, 0, 0, new System.Drawing.Size(imageWidth, imageHeight), CopyPixelOperation.SourceCopy);
            }            
            var resize = new Bitmap(bmp,new System.Drawing.Size(320,320));
            resize.Save("test.png", ImageFormat.Png);
            
        }
    }


}
