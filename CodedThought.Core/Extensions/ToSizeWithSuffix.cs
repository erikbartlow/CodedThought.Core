namespace CodedThought.Core.Extensions {

	public static class ToSizeWithSuffix {
		private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		/// <summary>Returns a string representation of the value with the largest available suffix.</summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string ToStringWithSizeSuffix(this Int64 value) {
			if (value < 0) { return "-" + ToStringWithSizeSuffix(-value); }
			if (value == 0) { return "0.0 bytes"; }

			int mag = (int)Math.Log(value, 1024);
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
		}

		/// <summary>Returns a string representation of the value with the largest available suffix.</summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string ToStringWithSizeSuffix(this Int32 value) {
			if (value < 0) { return "-" + ToStringWithSizeSuffix(-value); }
			if (value == 0) { return "0.0 bytes"; }

			int mag = (int)Math.Log(value, 1024);
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
		}

		/// <summary>Returns a string representation of the value with the largest available suffix.</summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string ToStringWithSizeSuffix(this Double value) {
			if (value < 0) { return "-" + ToStringWithSizeSuffix(-value); }
			if (value == 0) { return "0.0 bytes"; }

			int mag = (int)Math.Log(value, 1024);
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
		}

		/// <summary>Returns a string representation of the value with the largest available suffix.</summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string ToStringWithSizeSuffix(this float value) {
			if (value < 0) { return "-" + ToStringWithSizeSuffix(-value); }
			if (value == 0) { return "0.0 bytes"; }

			int mag = (int)Math.Log(value, 1024);
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
		}
	}
}