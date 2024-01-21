using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DrawAnywhere.Sys.WinApi;

namespace DrawAnywhere.Sys
{
    internal class ScreenCapture
    {
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

            if (autosave)
                SaveScreenshot(screenShot, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "DrawAnywhere!", $"{DateTime.Now}"));

            if (copyToClipboard)
                Clipboard.SetImage(screenShot);

            return screenShot;
        }

        public static void SaveScreenshot(Bitmap screenShot, string fileName)
        {
            screenShot.Save(fileName, ImageFormat.Png);
        }
    }
}
