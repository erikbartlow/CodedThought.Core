namespace CodedThought.Core.Data {
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public class HPDataAwareAssemblyAttribute : Attribute {
		private readonly bool _isDataAware;
		/// <summary>
		/// Gets whether this assembly contains CodedThought.Core data aware entities and controllers.
		/// </summary>
		public bool IsDataAware { get {  return _isDataAware; } }
		public HPDataAwareAssemblyAttribute() : this(false) { }
		/// <summary>
		/// Construct a new instance of the data aware assembly attribute with the string based boolean true/false.
		/// </summary>
		/// <param name="isDataAware"></param>
		public HPDataAwareAssemblyAttribute(bool isDataAware) {
			_isDataAware = isDataAware;
		}
	}
}
