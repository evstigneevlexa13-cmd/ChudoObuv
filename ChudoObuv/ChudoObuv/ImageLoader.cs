using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ChudoObuv
{
    public static class ImageLoader
    {
        private const string PlaceholderFileName = "picture.png";

        public static BitmapImage Load(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Open(PlaceholderFileName);
            }

            try
            {
                return Open(fileName);
            }
            catch (IOException)
            {
                return Open(PlaceholderFileName);
            }
        }

        private static BitmapImage Open(string fileName)
        {
            string outputPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                fileName);

            string fullPath = File.Exists(outputPath)
                ? outputPath
                : Path.GetFullPath(
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"..\..\Images",
                        fileName));

            return new BitmapImage(
                new Uri(fullPath, UriKind.Absolute));
        }
    }
}