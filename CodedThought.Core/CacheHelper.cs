using CodedThought.Core.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.Caching;

namespace CodedThought.Core {
    /// <summary> Temporary helper class for retrieving the current <see cref="HttpContext"/> . This temporary workaround should be removed in the future and <see cref="HttpContext"/> should be
    /// retrieved from the current controller, middleware, or page instead.
#if NET || NETCOREAPP
    /// If working in another component, the current
    /// <see cref="HttpContext" />
    /// can be retrieved from an
    /// <see cref="IHttpContextAccessor" />
    /// retrieved via dependency injection.
#endif

    /// </summary>
    public static class CacheHelper {

        /// <summary>Sets the passed object to <see cref="IMemoryCache" />.</summary>
        public static void AddToLocalCache(this runtime.MemoryCache cache, string key, object val) {
            var cacheExpiryOptions = new CacheItemPolicy {
                AbsoluteExpiration = DateTime.UtcNow.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2),
                Priority = System.Runtime.Caching.CacheItemPriority.Default
            };
            cache.Add(key, val, cacheExpiryOptions);
        }

        public static void AddToHttpCache(this IMemoryCache cache, string key, object val) {
            var cacheExpiryOptions = new MemoryCacheEntryOptions {
                AbsoluteExpiration = DateTime.UtcNow.AddMinutes(5),
                Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromMinutes(2),
                Size = 1024
            };
            cache.Set(key, val, cacheExpiryOptions);
        }

        /// <summary>Gets the requests object from <see cref="runtime.MemoryCache" />.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetFromLocalCache<T>(this runtime.MemoryCache cache, string key) {
            return (T)cache.Get(key);
        }

        /// <summary>Gets the requests object from <see cref="IMemoryCache" />.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetFromHttpCache<T>(this IMemoryCache cache, string key) {
            return cache.Get<T>(key);
        }
    }
}