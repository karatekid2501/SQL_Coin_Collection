using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CoinCollection
{
    /// <summary>
    /// Generic class for methods that do not need their own classes
    /// </summary>
    internal static class Misc
    {
        //Hard coded path for the image folder location
        private static readonly string _imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images");

        /// <summary>
        /// Creates a BitmapImage from path
        /// </summary>
        /// <param name="imageName">Name of the image</param>
        /// <returns>The generate BitmapImage. If no image is found, creates a blank image</returns>
        public static BitmapImage CreateImageFromPath(string imageName)
        {
            BitmapImage bitmapImage = new();

            if (!Path.Exists(Path.Combine(_imagePath, imageName)))
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new(Path.Combine(_imagePath, imageName), UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}
