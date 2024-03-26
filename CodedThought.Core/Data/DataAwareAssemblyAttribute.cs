namespace CodedThought.Core.Data {
	/// <summary>
	/// Defines an assembly as data aware for the CodedThought.Core.Data ORM management.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
	public sealed class DataAwareAssemblyAttribute : Attribute {
		/// 
		/// Summary:
		///		Gets whether this assembly contains CodedThought.Core data aware entities and controllers.
		/// Returns:
		///		A string representing a boolean value.  The reason for a string rather than a boolean is due 
		///		to the current Visual Studio limitations on assembly attributes only allowing string based parameters.
		public string IsDataAware { get; }
		/// <summary>
		/// Construct a new instance of the data aware assembly attribute with the string based boolean true/false.
		/// </summary>
		/// <param name="isDataAware"></param>
		public DataAwareAssemblyAttribute(string isDataAware) => IsDataAware = isDataAware;
	}
}
