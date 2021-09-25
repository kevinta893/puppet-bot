using LiteDB;
using PuppetBotClient.ImageCache.Models;
using PuppetBotClient.ImageCache.Util;
using System;
using System.IO;
using System.Linq;

namespace PuppetBotClient.ImageCache
{
    /// <summary>
    /// Stores a cached version of an image in a local database
    /// Can request an image from a http/https URL if cache missed
    /// </summary>
    public class ImageUrlCacher
    {
        private const string CacheDbPath = "emoji_cache.db";
        private const string CachedImageCollectionName = "CachedImages";

        private readonly LiteDatabase _db;

        private static ImageUrlCacher _instance;

        /// <summary>
        /// Initializes the URL cacher with a named db cache
        /// </summary>
        /// <param name="name"></param>
        public static void InitInstance(string name)
        {
            if (_instance == null)
            {
                _instance = new ImageUrlCacher(name);
            }
        }

        /// <summary>
        /// Gets the singleton instance of the url cacher
        /// </summary>
        public static ImageUrlCacher Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"{nameof(ImageUrlCacher)} needs to be initialized first before use.");
                }

                return _instance;
            }
        }

        /// <summary>
        /// Create a image cache. Note that this cache db cannot be
        /// shared across processes and name must be unique
        /// </summary>
        /// <param name="name"></param>
        private ImageUrlCacher(string name)
        {
            _db = new LiteDatabase($"{name}_{CacheDbPath}");

        }

        /// <summary>
        /// Returns a cached version of an image given the key
        /// Returns null if cache miss or no image found
        /// </summary>
        /// <returns></returns>
        public Stream GetCachedImage(string imageUrl)
        {
            var cache = _db.GetCollection<CachedImage>(CachedImageCollectionName);

            var img = cache.Find(img => img.ImageKey == imageUrl).FirstOrDefault();
            if (img == null)
            {
                //Cache miss, fetch and store new version
                var imageExtension = Path.GetExtension(imageUrl);
                var imageName = Path.GetFileName(imageUrl);

                // Get and store image
                using (var urlImageStream = UrlImageHelper.GetImageStreamFromUrl(imageUrl))
                {
                    StoreImageToCache(_db, imageUrl, imageName, urlImageStream);
                    urlImageStream.Close();
                }
            }

            // Get and store image
            var cachedStream = new MemoryStream();
            _db.FileStorage.Download(imageUrl, cachedStream);
            cachedStream.Seek(0, SeekOrigin.Begin);
            return cachedStream;
        }

        /// <summary>
        /// Stores an image into the cache
        /// </summary>
        private void StoreImageToCache(LiteDatabase dbContext, string imageKey, string fileName, Stream imageStream)
        {
            var cache = dbContext.GetCollection<CachedImage>(CachedImageCollectionName);

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

            dbContext.FileStorage.Upload(imageKey, fileName, imageStream);
            cache.Update(cacheRecord);
        }
    }
}
