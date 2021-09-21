using System.IO;
using System.Net;

namespace PuppetBotClient.ImageCache.Util
{
    public static class UrlImageHelper
    {
        public static Stream GetImageStreamFromUrl(string imageUrl)
        {
            using (var webClient = new WebClient())
            {
                var stream = webClient.OpenRead(imageUrl);
                var memBuff = new MemoryStream();
                stream.CopyTo(memBuff);
                memBuff.Seek(0, SeekOrigin.Begin);

                return memBuff;
            }
        }
    }
}
