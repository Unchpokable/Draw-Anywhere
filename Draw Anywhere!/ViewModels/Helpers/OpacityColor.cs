using System.Windows.Media;

namespace DrawAnywhere.ViewModels.Helpers
{
    /// <summary>
    /// Represents RGB color with separated opacity parameter which allows to control opacity and color separately
    /// </summary>
    internal class OpacityColor
    {
        public OpacityColor(Color color, float alpha)
        {
            if (color.A < byte.MaxValue)
                color.A = byte.MaxValue;

            BaseColor = color;
            Opacity = alpha;
        }

        public Color BaseColor { get; set; }
        public float Opacity { get; set; }

        public SolidColorBrush ToBrush()
        {
            var alpha = (byte)(Opacity * 255);
            return new SolidColorBrush(Color.FromArgb(alpha, BaseColor.R, BaseColor.G, BaseColor.B));
        }
    }
}
