using System;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using static DrawAnywhere.Sys.WinApi;

namespace DrawAnywhere.Extensions
{
    internal static class Extensions
    {
        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }

        public static ImageSource ToImageSource(this Bitmap bitmapSource)
        {
            return ImageSourceFromBitmap(bitmapSource);
        }
    }
}
