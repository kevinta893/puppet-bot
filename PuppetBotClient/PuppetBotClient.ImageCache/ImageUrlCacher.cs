using LiteDB;
using PuppetBotClient.ImageCache.Models;
using System;
using System.IO;
using System.Linq;

namespace PuppetBotClient.ImageCache
{
    /// <summary>
    /// Stores a cached version of an image in a local database
    /// </summary>
    public class ImageUrlCacher
    {
        private const string CacheDbPath = "emoji_cache.db";
        private const string CachedImageCollectionName = "CachedImages";

        public ImageUrlCacher()
        {
            
        }

        /// <summary>
        /// Returns a cached version of an image given the key
        /// Returns null if cache miss or no image found
        /// </summary>
        /// <returns></returns>
        public Stream GetCachedImage(string imageKey)
        {
            using (var db = new LiteDatabase(CacheDbPath))
            {
                var cache = db.GetCollection<CachedImage>(CachedImageCollectionName);

                var img = cache.Find(img => img.ImageKey == imageKey).FirstOrDefault();
                if (img == null)
                {
                    //Cache miss
                    return null;
                }

                Stream imageStream = new MemoryStream();
                db.FileStorage.Download(imageKey, imageStream);
                return imageStream;
            }
        }

        /// <summary>
        /// Stores an image into the cache
        /// </summary>
        public void StoreImageToCache(string imageKey, string fileName, Stream imageStream)
        {
            using (var db = new LiteDatabase(CacheDbPath))
            {
                var cache = db.GetCollection<CachedImage>(CachedImageCollectionName);

                // Check if record exists, otherwise insert
                var cacheRecord = cache.Find(img => img.ImageKey == imageKey).FirstOrDefault();
                if (cacheRecord == null)
                {
                    cacheRecord = new CachedImage()
                    {
                        ImageKey = imageKey,
                        LastUpdated = DateTime.Now,
                    };
                    
                    var insertedId = cache.Insert(cacheRecord);
                    cacheRecord.Id = insertedId;
                }

                cacheRecord.LastUpdated = DateTime.Now;

                db.FileStorage.Upload(imageKey, fileName, imageStream);
                cache.Update(cacheRecord);
            }
        }
    }
}
