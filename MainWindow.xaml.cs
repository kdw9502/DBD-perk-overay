using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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

        public bool isAutoUpdate = true;
        public bool hideWhenBackGround = true;
        private double perkDiplayingDelay = 5.0;


        public bool forceOff = false;

        int dpi = 96;
        readonly int originDpi = 96;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();


        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        public MainWindow()
        {
            InitializeComponent();
            GetCurrentDpi();
            PerkInfoLoader.load();
            
            StartRepositionTimer();
            ReadUserSettings();
            StartScanPerkAndDiplayTimer();
            
            ShowInTaskbar = true;
            Topmost = true;
        }

        private void StartScanPerkAndDiplayTimer()
        {
            perkScanTimer.Interval = TimeSpan.FromSeconds(perkDiplayingDelay);
            perkScanTimer.Tick += new EventHandler(AutoUpdate);
            perkScanTimer.Start();
        }

        private int nowDisplayingPerkIndex = 0;

        private void AutoUpdate(object sender, EventArgs e)
        {
            if (isAutoUpdate)
                UpdateGUIAndScan();
        }

        private void UpdateGUIAndScan()
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
                Description.Text = "희생제 진행중이 아닙니다.";
                PerkName.Text = "";

                return;
            }

            ShowNextPerk();
        }

        private void ShowNextPerk()
        {
            if (matchedPerkInfo.Count == 0)
                return;

            nowDisplayingPerkIndex++;

            if (nowDisplayingPerkIndex >= matchedPerkInfo.Count)
            {
                nowDisplayingPerkIndex = 0;
            }

            UpdateGUI(nowDisplayingPerkIndex);
        }

        private void ShowPrevPerk()
        {
            if (matchedPerkInfo.Count == 0)
                return;

            nowDisplayingPerkIndex--;

            if (nowDisplayingPerkIndex < 0)
            {
                nowDisplayingPerkIndex = matchedPerkInfo.Count - 1;
            }

            UpdateGUI(nowDisplayingPerkIndex);
        }

        private void HideOveray()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);

            if (dbdProcess.ProcessName != p.ProcessName)
            {
                PerkCanvas.Visibility = Visibility.Hidden;
                forceOff = false;
            }                
            else if (!forceOff)
            {
                PerkCanvas.Visibility = Visibility.Visible;
            }
                

        }

        private void UpdateGUI(int index)
        {
            var info = matchedPerkInfo[index];

            PerkImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}/resources/perks/{info.fileName}.png"));
            PerkName.Text = info.name;
            Description.Text = info.desc;
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
                SetProcessDPIAware();
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
            Top = rect.Top * originDpi / dpi;
            Left = rect.Left * originDpi / dpi;
            Height = rect.Height * originDpi / dpi;
            Width = rect.Width * originDpi / dpi;            
        }



        public void GetPerkImage()
        {
            int imageSize = 320;
            int screenshotSize = 320 * dpi / originDpi;
            
            var dpiSize = new System.Drawing.Size(screenshotSize * dpi / originDpi, screenshotSize * dpi / originDpi);

            screenshot = new Bitmap(screenshotSize, screenshotSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
         
            using (Graphics graphics = Graphics.FromImage(screenshot))
            {                
                graphics.CopyFromScreen((dbdRect.Right - screenshotSize), (dbdRect.Bottom -screenshotSize), 0, 0, new System.Drawing.Size(screenshotSize, screenshotSize), CopyPixelOperation.SourceCopy);
            }

            screenshot = new Bitmap(screenshot, new System.Drawing.Size(imageSize, imageSize));

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
            Description.LineHeight = settings.fontSIze + 4;
            perkDiplayingDelay = settings.displayTime;
            PerkColumn.Width = new GridLength(settings.perkSize);
            DescriptionColumn.Width = new GridLength(settings.width - settings.perkSize);
            hideWhenBackGround = settings.hide_when_background;
        }

        #region Toggle_On_off
        //https://social.technet.microsoft.com/wiki/contents/articles/30568.wpf-implementing-global-hot-keys.aspx

        private const int HOTKEY_ID_F1 = 9000;
        private const int HOTKEY_ID_F2 = 9001;
        private const int HOTKEY_ID_F3= 9002;
        private const int HOTKEY_ID_F4 = 9003;
        private const int HOTKEY_ID_F5 = 9004;
        private IntPtr _windowHandle;
        private HwndSource _source;
        private bool isInit = false;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            //https://docs.microsoft.com/ko-kr/dotnet/api/system.windows.forms.keys?view=netcore-3.1                        
            int f1_key = 112;
            int f2_key = 113;
            int f3_key = 114;
            int f4_key = 115;
            int f5_key = 116;

            RegisterHotKey(_windowHandle, HOTKEY_ID_F1, 0, f1_key);
            RegisterHotKey(_windowHandle, HOTKEY_ID_F2, 0, f2_key);
            RegisterHotKey(_windowHandle, HOTKEY_ID_F3, 0, f3_key);
            RegisterHotKey(_windowHandle, HOTKEY_ID_F4, 0, f4_key);
            RegisterHotKey(_windowHandle, HOTKEY_ID_F5, 0, f5_key);
            isInit = true;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            var key_id = wParam.ToInt32();
            if (msg == WM_HOTKEY )
            {
                if (key_id == HOTKEY_ID_F1)
                    OnPressF1();
                else if (key_id == HOTKEY_ID_F2)
                    OnPressF2();
                else if (key_id == HOTKEY_ID_F3)
                    OnPressF3();
                else if (key_id == HOTKEY_ID_F4)
                    OnPressF4();
                else if (key_id == HOTKEY_ID_F5)
                    OnPressF5();
            }           
            return IntPtr.Zero;
        }
        private void OnPressF1()
        {
            isAutoUpdate = !isAutoUpdate;
            RefreshIcon.Visibility = isAutoUpdate ? Visibility.Visible : Visibility.Hidden;
        }
        private void OnPressF2()
        {
            PerkCanvas.Visibility = PerkCanvas.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
            forceOff = PerkCanvas.Visibility == Visibility.Hidden;
        }

        private void OnPressF3()
        {
            ShowPrevPerk();
        }

        private void OnPressF4()
        {
            ShowNextPerk();
        }

        private void OnPressF5()
        {
            UpdateGUIAndScan();
        }

        protected override void OnClosed(EventArgs e)
        {
            if(isInit)
            {
                _source.RemoveHook(HwndHook);
                UnregisterHotKey(_windowHandle, HOTKEY_ID_F2);
                UnregisterHotKey(_windowHandle, HOTKEY_ID_F3);
                UnregisterHotKey(_windowHandle, HOTKEY_ID_F4);
            }


            base.OnClosed(e);
        }
        #endregion

        #region DPI 문제
        public void GetCurrentDpi()
        {
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            var dpiX = (int)dpiXProperty.GetValue(null, null);
            //var dpiY = (int)dpiYProperty.GetValue(null, null);

            this.dpi = dpiX;

        }

        #endregion
    }
}
