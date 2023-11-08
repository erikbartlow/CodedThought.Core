using CodedThought.Core.Validation;

using Microsoft.Extensions.Configuration;

namespace CodedThought.Core.Extensions {

	public static partial class Extensions {

		/// <summary>
		/// Loads a custom validation messages .json file to the builder.
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static IConfigurationBuilder AddValldationMessagesConfiguration(this IConfigurationBuilder builder, string settingsFileName)
		{
			builder.AddJsonFile(settingsFileName, optional: false, reloadOnChange: true);
			return builder;
		}

		public static TargetTypeEnum GetValueType(this object value) {
			if (value is Int32 || value is int || value is long) { return TargetTypeEnum.Numeric; }
			if (value is String || value is Char) { return TargetTypeEnum.Text; }
			if (value is Decimal || value is float || value is double) { return TargetTypeEnum.Numeric; }
			return value is DateTime ? TargetTypeEnum.Date : TargetTypeEnum.Text;
		}
	}
}