using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace LiveWallpaper
{
    /// <summary>
    /// Interaction logic for SettingsV2.xaml
    /// </summary>
    public partial class SettingsV2 : Window
    {
        private MainWindow mw = null;
        private bool skip = true;

        #region TEMPLATE
        bool allowClose = true;
        Timer winAero = new Timer();
        Timer checkForChange = new Timer();
        double previousWidth = 0;
        double previousHeight = 0;
        double previousTop = 0;
        double previousLeft = 0;

        public SettingsV2(MainWindow m)
        {
            mw = m;
            InitializeComponent();
            string[] FVI = FileVersionInfo.GetVersionInfo(currentDirectory + baseFileName).FileVersion.Split('.');
            release.Content = $"Release: {FVI[0]}.{FVI[1]}.{FVI[2]}";
            try
            {
                if (taskbarGroup != string.Empty)
                { SourceInitialized += (s, ev) => { TaskbarManager.Instance.SetApplicationIdForSpecificWindow(new WindowInteropHelper(this).Handle, taskbarGroup); }; }
            }
            catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show("Failed to set window ID"); mw.Close(); }
            windowBorder.Visibility = Visibility.Visible;
            appTitle.Content = windowTitle;
            if (!allowResize) { resizebtn.Visibility = Visibility.Collapsed; ResizeMode = ResizeMode.NoResize; }
            previousWidth = defaultWidth;
            previousHeight = defaultHeight;
            Width = defaultWidth;
            Height = defaultHeight;
            previousTop = Top;
            previousLeft = Left;
            if (minWidth > 0) { MinWidth = minWidth; }
            if (minHeight > 0) { MinHeight = minHeight; }
            if (maxWidth > 0) { MaxWidth = maxWidth; }
            if (maxHeight > 0) { MaxHeight = maxHeight; }
            WindowStartup();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            winAero.Interval = 10;
            winAero.Elapsed += checkForAeroFC;
            winAero.Start();

            DataContext = new XAMLStyles { };
            checkForChange.Interval = 1000;
            checkForChange.Elapsed += (se, ea) => { try { if (Styles.themeChanged) { Dispatcher.Invoke(() => { DataContext = new XAMLStyles { }; }); } } catch { } };
            checkForChange.Start();

            windowLoaded();
        }

        #region Window functions
        private void topBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Width == SystemParameters.WorkArea.Width && Height == SystemParameters.WorkArea.Height && allowResize == true)
                {
                    windowBorder.Visibility = Visibility.Visible;
                    Top = System.Windows.Forms.Control.MousePosition.Y - 15;
                    Left = System.Windows.Forms.Control.MousePosition.X - 400;
                    Width = previousWidth;
                    Height = previousHeight;
                    resizebtn.Content = "\uE922";
                    DragMove();
                }
                else if (e.ClickCount == 2 && allowResize == true)
                {
                    Top = 0;
                    Left = 0;
                    Width = SystemParameters.WorkArea.Width;
                    Height = SystemParameters.WorkArea.Height;
                    resizebtn.Content = "\uE923";
                    windowBorder.Visibility = Visibility.Hidden;
                }
                else
                {
                    DragMove();
                    previousWidth = Width;
                    previousHeight = Height;
                    previousTop = Top;
                    previousLeft = Left;
                }
            }
        }

        private void closebtn_Click(object sender, RoutedEventArgs e) { allowClose = true; Close(); }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (allowClose == false)
            {
                winAero.Stop();
                disallowClosing();
                e.Cancel = false;
            }
            else
            {
                winAero.Stop();
                allowClosing();
                e.Cancel = false;
            }
        }

        private void resizebtn_Click(object sender, RoutedEventArgs e)
        {
            if (Height != SystemParameters.WorkArea.Height && Width != SystemParameters.WorkArea.Width)
            {
                previousWidth = Width;
                previousHeight = Height;
                Top = 0;
                Left = 0;
                Height = SystemParameters.WorkArea.Height;
                Width = SystemParameters.WorkArea.Width;
                windowBorder.Visibility = Visibility.Hidden;
                resizebtn.Content = "\uE923";
            }
            else
            {
                WindowState = WindowState.Normal;
                Width = previousWidth;
                Height = previousHeight;
                Top = previousTop;
                Left = previousLeft;
                windowBorder.Visibility = Visibility.Visible;
                resizebtn.Content = "\uE922";
            }
        }

        private void minimisebtn_Click(object sender, RoutedEventArgs e) { allowClose = false; Close(); }

        private void checkForAeroFC(object sender, ElapsedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (allowResize)
                    {
                        if (WindowState == WindowState.Maximized)
                        {
                            WindowState = WindowState.Normal;
                            Top = 0;
                            Left = 0;
                            Width = SystemParameters.WorkArea.Width;
                            Height = SystemParameters.WorkArea.Height;
                            resizebtn.Content = "\uE923";
                            windowBorder.Visibility = Visibility.Hidden;
                        }
                        else if (Width != SystemParameters.WorkArea.Width && Height != SystemParameters.WorkArea.Height)
                        {
                            resizebtn.Content = "\uE922";
                            windowBorder.Visibility = Visibility.Visible;
                        }

                        if (Height > SystemParameters.WorkArea.Height) { Height = SystemParameters.WorkArea.Height; }
                    }
                    else
                    {
                        if (WindowState == WindowState.Maximized)
                        {
                            WindowState = WindowState.Normal;
                            Top = previousTop;
                            Left = previousLeft;
                            Width = defaultWidth;
                            Height = defaultHeight;
                            resizebtn.Content = "\uE923";
                            windowBorder.Visibility = Visibility.Visible;
                        }
                    }
                });
            }
            catch { }
        }
        #endregion
        #endregion

        #region TEMPLATE MODIFIERS
        string windowTitle = "Live Wallpaper";
        string baseFileName = "LiveWallpaper.dll";
        string currentDirectory = MainWindow.lwDIR;
        string taskbarGroup = "LiveWallpaper";
        bool allowResize = false;
        double defaultWidth = 325;
        double defaultHeight = 380;
        double minWidth = 325; //0 = No minimum
        double minHeight = 380; //0 = No minimum
        double maxWidth = SystemParameters.WorkArea.Width; //0 = No maximum
        double maxHeight = SystemParameters.WorkArea.Height; //0 = No maximum

        private void WindowStartup()
        {
            videoBorder.Visibility = Visibility.Visible;
        }

        private void windowLoaded()
        {
            getSettings();
            Activate();
        }

        #region Window closing
        private void allowClosing() { saveSettings(); foreach (Background bg in mw.backgrounds) { bg.Close(); } mw.Close(); }

        private void disallowClosing() { saveSettings(); }
        #endregion
        #endregion

        #region User Settings
        private void getSettings()
        {
            launchAtLogin.IsChecked = MainWindow.launchAtLogin;
            try
            {
                if (MainWindow.isVideo)
                {
                    preview.Source = new Uri(MainWindow.wallpaperSource);
                    volSlider.Value = MainWindow.volume;
                    foreach (Background bg in mw.backgrounds)
                    {
                        if (bg.display == MainWindow.primaryDisplay)
                        {
                            preview.Position = bg.wallpaperContent.Position;
                            break;
                        }
                    }
                }
                else
                {
                    volSlider.IsEnabled = false;
                    volText.IsEnabled = false;
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(MainWindow.wallpaperSource);
                    img.EndInit();
                    ImageBehavior.SetAnimatedSource(imagePreview, img);
                }
            }
            catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show("Failed to set media source."); mw.Close(); }

            skip = false;
        }

        private void saveSettings()
        {
            List<userSettings> toSave = new List<userSettings>();
            toSave.Add(new userSettings()
            {
                path = MainWindow.wallpaperSource,
                volume = MainWindow.volume,
                isVideo = MainWindow.isVideo,
                launchAtLogin = MainWindow.launchAtLogin
            });

            try
            {
                using (StreamWriter file = File.CreateText(MainWindow.lwDIR + "userSettings.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, toSave);
                }
            }
            catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show("Failed to save user settings."); }
        }
        #endregion

        #region Window UI Functions
        private void changeSource_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog o = new System.Windows.Forms.OpenFileDialog())
            {
                o.Title = "Select A Video/Image";
                o.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                o.Filter = "All Avaliable Files (*.MP4,*.MOV,*.MPG,*.FLV,*.BMP,*.JPG;*.PNG,*.TIFF,*.GIF)|*.MP4;*.MOV;*.MPG;*.FLV;*.BMP;*.JPG;*.PNG;*.TIFF;*.GIF|" +
                    "Videos (*.MP4,*.MOV,*.MPG,*.FLV)|*.MP4;*.MOV;*.MPG;*.FLV|" +
                    "Images (*.BMP,*.JPG;*.PNG,*.TIFF,*.GIF)|*.BMP;*.JPG;*.PNG;*.TIFF;*.GIF" /*+
                    "|All Files (*.*)|*.*"*/;
                if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        try
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromFile(o.FileName);
                            var img = new BitmapImage();
                            img.BeginInit();
                            img.UriSource = new Uri(o.FileName);
                            img.EndInit();
                            ImageBehavior.SetAnimatedSource(imagePreview, img);
                            preview.Source = null;
                            MainWindow.wallpaperSource = o.FileName;
                            foreach (Background ba in mw.backgrounds) { ImageBehavior.SetAnimatedSource(ba.imageBackground, img); ba.wallpaperContent.Source = null; }
                            MainWindow.isVideo = false;
                            volSlider.IsEnabled = false;
                            volText.IsEnabled = false;
                        }
                        catch
                        {
                            preview.Source = new Uri(o.FileName);
                            ImageBehavior.SetAnimatedSource(imagePreview, null);
                            MainWindow.wallpaperSource = o.FileName;
                            foreach (Background ba in mw.backgrounds) { ba.wallpaperContent.Source = new Uri(MainWindow.wallpaperSource); ImageBehavior.SetAnimatedSource(ba.imageBackground, null); }
                            MainWindow.isVideo = true;
                            volSlider.IsEnabled = true;
                            volText.IsEnabled = true;
                        }
                    }
                    catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show($"Failed to process file" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
            }
        }

        private void volSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.volume = volSlider.Value;
            mw.backgrounds[MainWindow.primaryDisplay].wallpaperContent.Volume = MainWindow.volume;
        }

        private void preview_MediaEnded(object sender, RoutedEventArgs e) { preview.Position = TimeSpan.FromSeconds(0); }

        private void launchAtLogin_Checked(object sender, RoutedEventArgs e)
        {
            if (skip == false)
            {
                MainWindow.launchAtLogin = true;
                try { CreateShortcut("kOFR Repo", Environment.GetFolderPath(Environment.SpecialFolder.Startup), AppDomain.CurrentDomain.BaseDirectory + "\\kOFR Repo.exe"); }
                catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show("Failed to add to startup."); skip = true; launchAtLogin.IsChecked = false; skip = false; }
            }
        }

        private void launchAtLogin_Unchecked(object sender, RoutedEventArgs e)
        {
            if (skip == false)
            {
                MainWindow.launchAtLogin = false;
                try { File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\kOFR Repo.lnk"); } catch { }
            }
        }

        public void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            try
            {
                string shortcutLocation = Path.Combine(shortcutPath, shortcutName + ".lnk");
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutLocation);
                shortcut.IconLocation = new Uri("https://kofr.000webhostapp.com/apps/kOFRRepo/Icon.ico").ToString();
                shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                shortcut.TargetPath = targetFileLocation;
                shortcut.Arguments = "-Hide -LaunchAppLiveWallpaper";
                shortcut.Save();
            }
            catch (Exception ex) { LogWriter.CreateLog(ex); MessageBox.Show("Failed to add to startup directory"); launchAtLogin.IsChecked = false; MainWindow.launchAtLogin = false; }
        }
        #endregion
    }
}
