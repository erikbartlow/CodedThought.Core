namespace CodedThought.Core.Data {

	[Flags]
	public enum DataTableUsage {
		ViewPriority = 1,
		IgnoreInherited = 2,
		ReadOnly = 4
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class DataTableUsageAttribute : Attribute {

		public DataTableUsageAttribute() {
		}

		public DataTableUsageAttribute(DataTableUsage useAs) => UseAs = useAs;

		public DataTableUsage UseAs { get; }

		public bool AllowMultiple => false;

		public bool Inherited => false;
	}
}