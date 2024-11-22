namespace CodedThought.Core.Data {

	[Flags]
	public enum DataTableUsage {
		ViewPriority = 1,
		IgnoreInherited = 2,
		ReadOnly = 4,
		OverrideInherited = 8
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class DataTableUsageAttribute : Attribute {

		public DataTableUsageAttribute() {
		}

		public DataTableUsageAttribute(DataTableUsage useAs)
		{
			UseAs = useAs;
			Inherited = false;
			AllowMultiple = false;
			OverrideInherited = false;

			if (UseAs.HasFlag(DataTableUsage.OverrideInherited))
				OverrideInherited = true;
		}

		public DataTableUsage UseAs { get; }

		public bool AllowMultiple { get; }

        public bool Inherited { get; }

        public bool OverrideInherited { get; }
	}
}