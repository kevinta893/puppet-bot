using System;

namespace PuppetBotClient.ImageCache.Models
{
    public class CachedImage
    {
        public int Id { get; set; }
        public string ImageKey { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
