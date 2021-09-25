using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PuppetBotClient.Util
{
    public static class UrlImageHelper
    {
        public static async Task<IBitmap> GetImageFromUrl(string imageUrl)
        {
            using (var webClient = new WebClient())
            {
                var stream = webClient.OpenRead(imageUrl);
                var bitmap = new System.Drawing.Bitmap(stream);
                var tempStream = new MemoryStream();
                bitmap.Save(tempStream, ImageFormat.Bmp);
                tempStream.Seek(0, SeekOrigin.Begin);

                var uiBitmap = new Bitmap(tempStream);

                stream.Flush();
                stream.Close();

                return uiBitmap;
            }
        }
    }
}
