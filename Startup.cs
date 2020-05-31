namespace LiveWallpaper
{
    public class Startup
    {
        public void tasks(string ex)
        {
            Styles.checkForChange();
            new MainWindow(ex).ShowDialog();
        }
    }
}
