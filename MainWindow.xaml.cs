using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DBD_perk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            StartTimer();
        }

        private void StartTimer()
        {
            
            timer.Interval = TimeSpan.FromMilliseconds(0.5);
            timer.Tick += new EventHandler(SetDbdOveray); 
            timer.Start();
        }

        private void SetDbdOveray(object sender, EventArgs e)
        {
            var windowFinder = new WindowFinder();
            var rect = windowFinder.GetDbdRect();
            //Debug.WriteLine($"{rect.Left}, {rect.Right}, {rect.Top}, {rect.Bottom}");

            SetRect(rect);
        }

        private void SetRect(WindowFinder.Rect rect)
        {

            Top = rect.Top;
            Left = rect.Left;
            Height = rect.Bottom - rect.Top;
            Width = rect.Right - rect.Left;
            Topmost = true;
        }
    }


}
