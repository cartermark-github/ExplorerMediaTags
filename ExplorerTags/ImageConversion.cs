using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ExplorerTags
{
    public static class ImageConversion
    {
        /// <summary>
        /// Converts a System.Drawing.Bitmap to a WPF BitmapSource.
        /// </summary>
        public static BitmapSource ToBitmapSource(Bitmap bitmap)
        {
            // Use a MemoryStream to save the Bitmap as a PNG, then load it back as a BitmapSource.
            using (MemoryStream stream = new MemoryStream())
            {
                // Save the bitmap to the stream in PNG format
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                // Create a BitmapImage (a type of BitmapSource) and set its source
                BitmapImage bitmapImage = new BitmapImage();

                // Begin the initialization process
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                // Set cache option to prevent the stream from being held open
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                // End the initialization process
                bitmapImage.EndInit();

                // Freeze the image for better performance and thread safety
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
