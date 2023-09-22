using CodedThought.Core.Validation;

namespace CodedThought.Core.Extensions {

	public static partial class Extensions {

		public static TargetTypeEnum GetValueType(this object value) {
			if (value is Int32 || value is int || value is long) { return TargetTypeEnum.Numeric; }
			if (value is String || value is Char) { return TargetTypeEnum.Text; }
			if (value is Decimal || value is float || value is double) { return TargetTypeEnum.Numeric; }
			return value is DateTime ? TargetTypeEnum.Date : TargetTypeEnum.Text;
		}
	}
}