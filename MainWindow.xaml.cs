using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DBD_perk
{
    public partial class MainWindow : System.Windows.Window
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
        DispatcherTimer perkScanTimer = new DispatcherTimer();
        Process dbdProcess;
        IntPtr dbdHandle;
        Rect dbdRect;
        Bitmap screenshot;
        readonly string dbdProcessName = "DeadByDaylight-Win64-Shipping";


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        //[DllImport("user32.dll")]
        //static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);


        public MainWindow()
        {
            InitializeComponent();

            PerkInfoLoader.load();
            
            StartRepositionTimer();
            StartScanPerkAndDiplayTimer();
            ShowInTaskbar = true;
            Topmost = true;
        }
        private readonly double perkDiplayingDelay = 5.0;
        private void StartScanPerkAndDiplayTimer()
        {
            perkScanTimer.Interval = TimeSpan.FromSeconds(perkDiplayingDelay);
            perkScanTimer.Tick += new EventHandler(UpdateGUIAndScan);
            perkScanTimer.Start();
        }

        private int nowDisplayingPerkIndex = 0;
        private void UpdateGUIAndScan(object sender, EventArgs e)
        {
            var perkCount = matchedPerkInfo.Count;


            if (perkCount == 0)
            {
                GetPerkImage();
                FindOutPerks();
                PerkImage.Source = null;
                Description.Text = "게임 진행중이 아닙니다.";

                //HideWhenDBDIsBackGround();

                return;
            }
            

            UpdateGUI(nowDisplayingPerkIndex);

            bool isLastPerk = nowDisplayingPerkIndex + 1 == perkCount;
            if (isLastPerk)
            {
                GetPerkImage();
                FindOutPerks();
                nowDisplayingPerkIndex = -1;
            }
            nowDisplayingPerkIndex++;            
        }

        private void HideWhenDBDIsBackGround()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);

            if (dbdProcess.Id != p.Id)
                PerkCanvas.Visibility = Visibility.Hidden;
            else
                PerkCanvas.Visibility = Visibility.Visible;

        }

        private void UpdateGUI(int index)
        {
            PerkImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}/resources/perks/{matchedPerkInfo[index].fileName}.png"));          

            Description.Text = matchedPerkInfo[index].desc;
        }


        private void StartRepositionTimer()
        {
            UpdateDbdOveray(null, null);

            repositionTimer.Interval = TimeSpan.FromSeconds(0.5);
            repositionTimer.Tick += new EventHandler(UpdateDbdOveray); 
            repositionTimer.Start();            
        }


        private void UpdateDbdOveray(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName(dbdProcessName);

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
            int imageWidth = 320;
            int imageHeight = 320;

            screenshot = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(screenshot))
            {
                
                graphics.CopyFromScreen(dbdRect.Left + dbdRect.Width - imageWidth, dbdRect.Top + dbdRect.Height -imageHeight, 0, 0, new System.Drawing.Size(imageWidth, imageHeight), CopyPixelOperation.SourceCopy);
            }            

        }

        private List<PerkInfo> matchedPerkInfo = new List<PerkInfo>();
        public void FindOutPerks()
        {
            var infos = PerkInfoLoader.infos;
            Mat screenshotMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);
            Cv2.CvtColor(screenshotMat, screenshotMat, ColorConversionCodes.BGR2GRAY);

            matchedPerkInfo.Clear();

            foreach (var info in infos)
            {
                var (matched,matchRate) = PerkImageMatcher.match(screenshotMat, info.image);
                info.matchRate = matchRate;
                if (matched)
                {               
                    matchedPerkInfo.Add(info);
                }            
            }            
        }

    }

}
