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
		/// <remarks>This method will set the cache item to absolutely expire in 60 minutes.</remarks>
		public static void AddToLocalCache(this runtime.MemoryCache cache, string key, object val) {
			CacheItemPolicy cacheExpiryOptions = new CacheItemPolicy {
				AbsoluteExpiration = DateTime.UtcNow.AddMinutes(60),
				Priority = System.Runtime.Caching.CacheItemPriority.Default
			};
			cache.Add(key, val, cacheExpiryOptions);
		}
		public static void AddToLocalCache(this runtime.MemoryCache cache, string key, object val, CacheItemPolicy itemPolicy) => cache.Add(key, val, itemPolicy);
		/// <summary>
		/// Adds an item to the <see cref="IMemoryCache"/>
		/// </summary>
		/// <remarks>This method will set the cache item to absolutely expire in 60 minutes.</remarks>
		/// <param name="cache"></param>
		/// <param name="key"></param>
		/// <param name="val"></param>
		public static void AddToHttpCache(this IMemoryCache cache, string key, object val) {
			MemoryCacheEntryOptions cacheExpiryOptions = new MemoryCacheEntryOptions {
				AbsoluteExpiration = DateTime.UtcNow.AddMinutes(60),
				Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.High,
				Size = 1024
			};
			cache.Set(key, val, cacheExpiryOptions);
		}
		/// <summary>
		/// Adds an item to the <see cref="IMemoryCache"/> with the passed cache entry options policy.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <param name="itemPolicy"></param>
		public static void AddToHttpCache(this IMemoryCache cache, string key, object val, MemoryCacheEntryOptions itemPolicy) => cache.Set(key, val, itemPolicy);
		/// <summary>Gets the requests object from <see cref="runtime.MemoryCache" />.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public static T GetFromLocalCache<T>(this runtime.MemoryCache cache, string key) => (T)cache.Get(key);

		/// <summary>Gets the requests object from <see cref="IMemoryCache" />.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public static T GetFromHttpCache<T>(this IMemoryCache cache, string key) => cache.Get<T>(key);
		/// <summary>
		/// Gets the item associated with the key if present.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cache"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryGetValue<TItem>(this runtime.MemoryCache cache, string key, out TItem? value) {
			object valueCache = cache.Get(key);
			if (valueCache == null) {
				value = default;
				return false;
			}
			if (valueCache is TItem item) {
				value = item;
				return true;
			}
			value = default;
			return false;
		}
	}
}