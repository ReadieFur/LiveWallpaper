using System;
using System.Windows;
using System.Windows.Interop;
using System.Threading;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace LiveWallpaper
{
    public partial class Background : Window
    {
        public int display = 0;
        internal MainWindow mw = null;

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                TaskbarManager.Instance.SetApplicationIdForSpecificWindow(new WindowInteropHelper(this).Handle, "LiveWallpaper");
                Window_Properties.disableActivation(this);
            }
            catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show("Failed to set window ID"); mw.Close(); }
        }

        public Background(int disp, MainWindow m)
        {
            mw = m;
            InitializeComponent();
            display = disp;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string displayParams = MainWindow.displays[display].ToString();
                string[] split = displayParams.Split(',');
                Left = Convert.ToInt32(split[0]);
                Top = Convert.ToInt32(split[1]);
                Width = Convert.ToInt32(split[2]);
                Height = Convert.ToInt32(split[3]);
                wallpaperContent.Margin = new Thickness(0, 0, Width - Convert.ToInt32(split[4]), Height - Convert.ToInt32(split[5]));
                imageBackground.Margin = new Thickness(0, 0, Width - Convert.ToInt32(split[4]), Height - Convert.ToInt32(split[5]));
            }
            catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show("Failed to get display properties."); mw.Close(); }

            try { Window_Properties.setBottom(this); } catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show("Failed to set app Z-axis."); mw.Close(); }

            try
            {
                if (display == MainWindow.primaryDisplay) { wallpaperContent.Volume = 0; }
                if (MainWindow.isVideo) { wallpaperContent.Source = new Uri(MainWindow.wallpaperSource); }
                else
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(MainWindow.wallpaperSource);
                    img.EndInit();
                    ImageBehavior.SetAnimatedSource(imageBackground, img);
                }
            }
            catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show("Failed to set wallpaper."); mw.Close(); }

            System.Timers.Timer keepBack = new System.Timers.Timer();
            keepBack.Interval = 500;
            keepBack.Elapsed += (se, ev) => { try { if (MainWindow.resetBack) { Thread.Sleep(50); Dispatcher.Invoke(() => { Window_Properties.setBottom(this); }); } } catch { } };
            keepBack.Start();
        }

        private void wallpaperContent_MediaEnded(object sender, RoutedEventArgs e) { wallpaperContent.Position = TimeSpan.FromSeconds(0); }
    }
}
