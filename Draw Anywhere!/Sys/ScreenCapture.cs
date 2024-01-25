using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
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
            if (!GetCursorPos(out POINT cursorPosition))
            {
                return null;
            }
            IntPtr monitorHandle = MonitorFromPoint(cursorPosition, 2);

            var mi = new MONITORINFO();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            if (!GetMonitorInfo(monitorHandle, ref mi))
                return null;

            var monitorRect = mi.rcMonitor;

            var screenBounds = new Rectangle(new Point(monitorRect.left, monitorRect.top),
            new Size()
            {
                Width = monitorRect.right - monitorRect.left,
                Height = monitorRect.bottom - monitorRect.top
            });

            Bitmap screenShot = new Bitmap(screenBounds.Width, screenBounds.Height);
            using (Graphics g = Graphics.FromImage(screenShot))
            {
                g.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size);
            }

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

            screenShot.Dispose();
        }

        private static void SaveScreenshotWithNotification(Bitmap screenshot)
        {
            var path = _config.ScreenShotPath;

            SaveScreenshot(screenshot, Path.Combine(path, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}-{new Random().Next() / 1024}.png"));

            ViewModels.Notifications.ShowSuccess("Done!", $"ScreenShot successfully saved at {path}",
                () =>
                {
                    try
                    {
                        Process.Start("explorer.exe", path);
                    }
                    catch (Exception ex)
                    {
                        ViewModels.Notifications.ShowError("Screenshot directory opening fail", ex.Message);
                    }
                });
        }
    }
}
