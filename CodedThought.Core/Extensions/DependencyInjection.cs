using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using CodedThought.Core.Data;
using CodedThought.Core.Data.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace CodedThought.Core.Extensions
{
	public static class DataProviderDependencyInjection
	{
		/// <summary>
		/// Add the passed <see cref="IDatabaseObject"/> provider type as a transient service.
		/// </summary>
		/// <typeparam name="TService"></typeparam>
		/// <param name="services"></param>
		/// <remarks>
		/// Providers hold a connection and an optional transaction, so each data operation receives its own instance.  Sharing a single instance across concurrent callers corrupts both.
		/// </remarks>
		/// <returns></returns>
		public static IServiceCollection AddCoreDataProvider<TService>(this IServiceCollection services) where TService : class
		{
			services.AddSingleton(CreateProviderRegistration(typeof(TService), null));
			services.AddTransient(typeof(IDatabaseObject), typeof(TService));
			return services;

		}
		/// <summary>
		/// Add the passed <see cref="IDatabaseObject"/> provider type as a keyed transient service.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="dataProvider"></param>
		/// <param name="key"></param>
		/// <remarks>See AddKeyedTransient for details.</remarks>
		/// <returns></returns>
		public static IServiceCollection AddCoreDataProvider<TService>(this IServiceCollection services, object key) where TService : class
		{
			services.AddSingleton(CreateProviderRegistration(typeof(TService), key));
			services.AddKeyedTransient(typeof(IDatabaseObject), key, typeof(TService));
			return services;
		}

		/// <summary>
		/// Creates the registration describing the provider type.  The provider is instantiated once here so the database it supports is known without having to create an instance on every lookup.
		/// </summary>
		/// <param name="providerType"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		private static CoreDataProviderRegistration CreateProviderRegistration(Type providerType, object? key)
		{
			if (!typeof(IDatabaseObject).IsAssignableFrom(providerType))
				throw new ArgumentException($"The provider type, {providerType.FullName}, does not implement {nameof(IDatabaseObject)}.", nameof(providerType));

			using IDatabaseObject probe = (IDatabaseObject) Activator.CreateInstance(providerType)!;
			return new CoreDataProviderRegistration(providerType, probe.SupportedDatabase, key);
		}
	}
}
