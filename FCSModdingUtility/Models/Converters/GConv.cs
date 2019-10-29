using System.Windows.Media;

namespace FCSModdingUtility
{
    /// <summary>
    /// A class of generic converters
    /// </summary>
    public class GConv
    {
        /// <summary>
        /// Converts RGB to SolidColorBrush
        /// </summary>
        /// <param name="r">Red hex</param>
        /// <param name="g">Green hex</param>
        /// <param name="b">Blue hex</param>
        /// <returns></returns>
        public static SolidColorBrush RGB2SolidColorBrush(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
