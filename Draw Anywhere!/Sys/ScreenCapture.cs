using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using DrawAnywhere.Models;
using DrawAnywhere.SFX;
using static DrawAnywhere.Sys.WinApi;

namespace DrawAnywhere.Sys
{
    internal class ScreenCapture
    {
        static ScreenCapture()
        {
            _config = AppConfig.Instance();
            _sfx = new SfxProvider();
        }

        private static SfxProvider _sfx;
        private static AppConfig _config;
        public static Bitmap MakeScreenshot(bool autosave = true, bool copyToClipboard = false)
        {
            IntPtr desktopWindow = GetDesktopWindow();
            IntPtr desktopDC = GetWindowDC(desktopWindow);
            Point cursorPosition = Cursor.Position;
            Rectangle screenBounds = Screen.FromPoint(cursorPosition).Bounds;

            Bitmap screenShot = new Bitmap(screenBounds.Width, screenBounds.Height);
            using (Graphics g = Graphics.FromImage(screenShot))
            {
                IntPtr hDestDC = g.GetHdc();
                BitBlt(hDestDC, 0, 0, screenBounds.Width, screenBounds.Height, desktopDC, 0, 0, CopyPixelOperation.SourceCopy);
                g.ReleaseHdc(hDestDC);
            }

            ReleaseDC(desktopWindow, desktopDC);

            _sfx.PlayCamera();

            if (autosave)
            {
                SaveScreenshotWithNotification(screenShot);
            }

            if (copyToClipboard)
                Clipboard.SetImage(screenShot);

            return screenShot;
        }

        public static void SaveScreenshot(Bitmap screenShot, string fileName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            if (File.Exists(fileName))
            {
                screenShot.Save(fileName, ImageFormat.Png);
                return;
            }

            using var stream = new FileStream(fileName, FileMode.OpenOrCreate);
            screenShot.Save(stream, ImageFormat.Png);
        }

        private static void SaveScreenshotWithNotification(Bitmap screenshot)
        {
            var path = _config.ScreenShotPath;

            SaveScreenshot(screenshot, Path.Combine(path, $"screenshot_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}-{new Random().Next() / 1024}.png"));

            ViewModels.Notifications.ShowSuccess("Done!", $"ScreenShot successfully saved at {path}",
                () =>
                {
                    try
                    {
                        Process.Start(path);
                    }
                    catch (Exception ex)
                    {
                        ViewModels.Notifications.ShowError("Screenshot directory opening fail", ex.Message);
                    }
                });
        }
    }
}
