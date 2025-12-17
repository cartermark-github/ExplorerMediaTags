using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ExplorerTags
{
    public static class APICodePackHelpers
    {
        public static Bitmap GetVideoThumbnail(string videoFilePath)
        {
            // 1. Create a ShellObject from the video file path
            ShellFile shellFile = null;
            try
            {
                shellFile = ShellFile.FromFilePath(videoFilePath);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Error: File not found at {videoFilePath}");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating ShellFile: {ex.Message}");
                return null;
            }

            // 2. Access the Thumbnail property and retrieve the Bitmap
            // You can choose from SmallIcon, MediumIcon, LargeIcon, ExtraLargeIcon, or Bitmap 
            // The .Bitmap property provides the highest available resolution for the thumbnail.
            Bitmap thumbnailBitmap = null;
            if (shellFile.Thumbnail != null)
            {
                // Get the bitmap
                thumbnailBitmap = shellFile.Thumbnail.Bitmap;
            }

            // Note: The ShellFile object must be disposed of when finished.
            shellFile.Dispose();

            return thumbnailBitmap;
        }

    }
}
