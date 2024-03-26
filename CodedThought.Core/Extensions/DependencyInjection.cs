using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using CodedThought.Core.Data.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace CodedThought.Core.Extensions
{
	public static class DataProviderDependencyInjection
	{
		/// <summary>
		/// Add the passed <see cref="IDatabaseObject"/> provider type as a singleton.
		/// </summary>
		/// <typeparam name="TService"></typeparam>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddCoreDataProvider<TService>(this IServiceCollection services) where TService : class
		{
			TService implementation = (TService) Activator.CreateInstance(typeof(TService));
			services.AddSingleton(typeof(IDatabaseObject), implementation);
			return services;

		}
		/// <summary>
		/// Add the passed <see cref="IDatabaseObject"/> provider type as a keyed singleton
		/// </summary>
		/// <param name="services"></param>
		/// <param name="dataProvider"></param>
		/// <param name="key"></param>
		/// <remarks>See AddKeyedSingleton for details.</remarks>
		/// <returns></returns>
		public static IServiceCollection AddCoreDataProvider<TService>(this IServiceCollection services, object key) where TService : class
		{
			TService implementation = (TService) Activator.CreateInstance(typeof(TService));
			services.AddKeyedSingleton(typeof(IDatabaseObject), key, implementation);
			return services;
		}

	}
}
