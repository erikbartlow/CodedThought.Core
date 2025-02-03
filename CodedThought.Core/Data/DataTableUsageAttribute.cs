namespace CodedThought.Core.Data {

	[Flags]
	public enum DataTableUsage {
		ViewPriority = 1,
		IgnoreInherited = 2,
		ReadOnly = 4,
		OverrideInherited = 8,
        AutoGenerateUniqueIdentifier = 16
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
			AutoGenerateUniqueIdentifier = false;
			ReadOnly = false;

			if (useAs.HasFlag(DataTableUsage.OverrideInherited))
				OverrideInherited = true;
			if (useAs.HasFlag(DataTableUsage.AutoGenerateUniqueIdentifier))
				AutoGenerateUniqueIdentifier = true;
			if( useAs.HasFlag(DataTableUsage.ReadOnly))
				ReadOnly = true;
		}

		public DataTableUsage UseAs { get; }

		public bool AllowMultiple { get; }

        public bool Inherited { get; }

        public bool OverrideInherited { get; }
		public bool AutoGenerateUniqueIdentifier { get; }	

		public bool ReadOnly { get; }
	}
}