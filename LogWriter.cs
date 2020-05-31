using System;
using System.IO;

namespace LiveWallpaper
{
    class LogWriter
    {
        public static void CreateLog(Exception logToWrite)
        {
            try
            {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Logs.txt"))
                {
                    using (StreamWriter sw = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "Logs.txt"))
                    {
                        sw.Write("Logs for LiveWallpaper.dll.\n" +
                            "Events are sorted from oldest to newest.\n" +
                            "===========================================================");
                    }
                }

                using (StreamWriter sw = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "\\Logs.txt"))
                {
                    sw.WriteLine($"\n\nLog Entry: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
                    //sw.WriteLine($"Error Type: {}");
                    sw.WriteLine($"Log Details: {logToWrite}");
                    sw.Write("-------------------------------");
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Failed to create log.", "Log Error");
                Environment.Exit(0);
            }
        }
    }
}
