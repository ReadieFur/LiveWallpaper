using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json;

namespace LiveWallpaper
{
    public partial class MainWindow : Window
    {
        public static List<string> displays = new List<string>();
        public static string wallpaperSource = string.Empty;
        public static double volume;
        public static string lwDIR;
        public static NotifyIcon notifyIcon = null;
        public static int primaryDisplay = -1;
        public static bool resetBack = false;
        public static bool closeApplication = false;
        //public static TimeSpan primaryVideoElapsed = TimeSpan.FromSeconds(0); //WIP Syncronisation
        public static bool isVideo = false;
        public List<Background> backgrounds = new List<Background>();
        public static bool launchAtLogin = false;

        protected override void OnClosing(CancelEventArgs e)
        {
            notifyIcon.Icon = null;
            notifyIcon.Visible = false;
            e.Cancel = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            TaskbarManager.Instance.SetApplicationIdForSpecificWindow(new WindowInteropHelper(this).Handle, "LiveWallpaper");
            Window_Properties.disableActivation(this);
        }

        public MainWindow(string ex)
        {
            getWorkingDIR();

            InitializeComponent();
            testContent.Volume = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;

            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/LiveWallpaper;component/Resources/Icon.ico")).Stream;
            notifyIcon = new NotifyIcon();
            notifyIcon.Click += new EventHandler(notifyIcon_Click);
            notifyIcon.Icon = new Icon(iconStream);
            notifyIcon.Text = "Live Wallpaper";
            notifyIcon.Visible = true;

            loadUserSettings();

            try
            {
                int displayCount = -1;
                foreach (var screen in Screen.AllScreens)
                {
                    displayCount += 1;
                    if (screen.Primary.ToString() == "True") { primaryDisplay = displayCount; }
                    Rectangle bo = screen.Bounds;
                    Rectangle wa = screen.WorkingArea;
                    displays.Add($"{wa.X},{wa.Y},{wa.Width},{wa.Height},{bo.Width},{bo.Height}");
                    Background bkg = new Background(displayCount, this);
                    backgrounds.Add(bkg);
                    bkg.Show();
                }
            }
            catch (Exception ex) { LogWriter.CreateLog(ex); System.Windows.MessageBox.Show("Failed to startup application.", "Livewallpaper Error"); Close(); }
        }

        private void loadUserSettings()
        {
            try
            {
                List<userSettings> toLoad = JsonConvert.DeserializeObject<List<userSettings>>(File.ReadAllText(lwDIR + "userSettings.json"));
                foreach (userSettings userSetting in toLoad)
                {
                    testContent.Source = new Uri(userSetting.path); //Test if file is valid

                    wallpaperSource = userSetting.path;
                    volume = userSetting.volume;
                    isVideo = userSetting.isVideo;
                    try { launchAtLogin = userSetting.launchAtLogin; } catch { }
                }
            }
            catch (Exception ex)
            {
                LogWriter.CreateLog(ex);
                try { File.Delete(lwDIR + "userSettings.json"); } catch { }
                try
                {
                    const string userRoot = "HKEY_CURRENT_USER";
                    const string WsubKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers";
                    const string WkeyName = userRoot + "\\" + WsubKey;
                    testContent.Source = new Uri(Registry.GetValue(WkeyName, "BackgroundHistoryPath0", string.Empty).ToString());
                    wallpaperSource = new Uri(Registry.GetValue(WkeyName, "BackgroundHistoryPath0", string.Empty).ToString()).ToString().Substring(8).Replace('/', '\\');
                    testContent.Source = null;
                }
                catch (Exception ex1)
                {
                    //System.Windows.MessageBox.Show("Well done T, You fucked it lol. -My dev testing messages are nice. Also you shouldn't be seeing this.");
                    wallpaperSource = null;
                    LogWriter.CreateLog(ex1);
                }
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            resetBack = true;
            var sw = new SettingsV2(this);
            sw.Loaded += (se, ev) => { notifyIcon.Visible = false; };
            sw.Closed += (se, ev) => { notifyIcon.Visible = true; resetBack = false; };
            sw.Show();
        }

        private void getWorkingDIR()
        {
            List<string> paths = new List<string>();
            foreach (string f in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory))
            {
                paths.Add(f);
            }

            foreach (string d in Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory))
            {
                foreach (string f in Directory.GetFiles(d))
                {
                    paths.Add(f);
                }
            }

            string dir = string.Empty;

            foreach (string s in paths)
            {
                if (s.Contains("LiveWallpaper.dll"))
                {
                    List<string> path = s.Split('\\').ToList();
                    path.RemoveAt(path.Count - 1);
                    foreach (string d in path)
                    {
                        dir = dir + d + "\\";
                    }
                    break;
                }
                else { }
            }

            lwDIR = dir;
        }
    }
}
