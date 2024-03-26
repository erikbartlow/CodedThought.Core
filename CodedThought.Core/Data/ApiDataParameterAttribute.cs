namespace CodedThought.Core.Data {

	public enum ApiDataParameterOptions {

		/// <summary>Informs the framework that the attribute is to be used as the default parameter during Api calls.</summary>
		DefaultParameter = 1,

		/// <summary>Causes the framework to establish this attribute in the ORM over any inherited attributes for this same property.</summary>
		OverridesInherited = 2
	}

	/// <summary>Maps a property to a Database Column or XML Element</summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ApiDataParameterAttribute : Attribute {

		#region Properties

		/// <summary>Used in the Generic Api Data Controller to know the parameter name to pass.</summary>
		public string ParameterName { get; set; }

		/// <summary>The type of property in the object.</summary>
		public Type? PropertyType { get; set; }

		/// <summary>The name of the property desribed by the Api Parameter attribute.</summary>
		public string? PropertyName { get; set; }

		/// <summary>Applies additional options for the parameter.</summary>
		public ApiDataParameterOptions Options { get; set; }

		#endregion Properties

		/// <summary>Initializes an Api Data Parameter for this property. The name should match the arguement name passed to a REST Api.</summary>
		/// <param name="name"></param>
		/// <example>https://url/controller/action? <see cref="ParameterName" />?=value</example>
		public ApiDataParameterAttribute(string name) => ParameterName = name;

		/// <summary>Initializes an Api Data Parameter for this property including any option flags.</summary>
		/// <param name="name">   </param>
		/// <param name="options"><see cref="ApiDataParameterOptions" /></param>
		/// <example>https://url/controller/action? <see cref="ParameterName" />?=value</example>
		public ApiDataParameterAttribute(string name, ApiDataParameterOptions options) : this(name) => Options = options;
	}
}