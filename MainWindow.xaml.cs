using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;

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
        public static Rect dbdRect;
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
            ReadUserSettings();
            StartScanPerkAndDiplayTimer();
            ShowInTaskbar = true;
            Topmost = true;
        }

        public bool hideWhenBackGround = true;
        private double perkDiplayingDelay = 5.0;
        private void StartScanPerkAndDiplayTimer()
        {
            perkScanTimer.Interval = TimeSpan.FromSeconds(perkDiplayingDelay);
            perkScanTimer.Tick += new EventHandler(UpdateGUIAndScan);
            perkScanTimer.Start();
        }

        private int nowDisplayingPerkIndex = 0;
        private void UpdateGUIAndScan(object sender, EventArgs e)
        {           

            GetPerkImage();
            FindOutPerks();

            if (hideWhenBackGround)
            {
                HideOveray();
            }

            if (matchedPerkInfo.Count == 0)
            {
                PerkImage.Source = null;
                Description.Text = "게임 진행중이 아닙니다.";                

                return;
            }

            if (nowDisplayingPerkIndex >= matchedPerkInfo.Count)
            {
                nowDisplayingPerkIndex = 0;
            }

            UpdateGUI(nowDisplayingPerkIndex);

            nowDisplayingPerkIndex++;            
        }

        private void HideOveray()
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
                this.Close();
                MessageBox.Show("실행중인 데바데를 찾지 못했습니다. 프로그램을 종료합니다.");                
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

        public class Settings
        {
            public int fontSIze { get; set; } = 18;
            public int width { get; set; } = 1100;
            public int height { get; set; } = 192;
            public double displayTime { get; set; } = 5.0;
            public int perkSize { get; set; } = 128;
            public bool hide_when_background { get; set; } = true;

            public void AdjustLimit()
            {
                fontSIze = TrimInt(fontSIze, 10, 30);
                perkSize = TrimInt(perkSize, 64, 512);
                width = TrimInt(width, 512, 3840);
                height = TrimInt(height, perkSize + fontSIze + 2, 2160);
                displayTime = TrimDouble(displayTime, 1.0, 15.0);
            }
            private int TrimInt(int value,int min, int max)
            {
                if (value < min)
                    return min;
                if (value > max)
                    return max;
                return value;
            }

            private double TrimDouble(double value, double min, double max)
            {
                if (value < min)
                    return min;
                if (value > max)
                    return max;
                return value;
            }
        }

        private void ReadUserSettings()
        {

            var settings = new Settings();

            try
            {
                var jsonUtf8Bytes = File.ReadAllBytes($"settings.txt");
                var readOnlySpan = new ReadOnlySpan<byte>(jsonUtf8Bytes);
                settings = JsonSerializer.Deserialize<Settings>(readOnlySpan);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("설정 파일을 읽을 수 없습니다. 기본 설정을 복구합니다.");
            }
            catch (IOException)
            {
                MessageBox.Show("설정 파일이 다른 프로그램에서 사용중입니다. 다른 프로그램을 끄고 다시 실행해주세요.");
            }
            settings.AdjustLimit();
            SetUserSettings(settings);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var text = JsonSerializer.Serialize(new Settings(), options);
            File.WriteAllText("settings.txt", text);

            SetUserSettings(settings);
        }
        private void SetUserSettings(Settings settings)
        {
            PerkCanvas.Width = settings.width;
            PerkCanvas.Height = settings.height;
            Description.FontSize = settings.fontSIze;
            perkDiplayingDelay = settings.displayTime;
            PerkColumn.Width = new GridLength(settings.perkSize);
            DescriptionColumn.Width = new GridLength(settings.width - settings.perkSize);
            hideWhenBackGround = settings.hide_when_background;
        }

    }
}
