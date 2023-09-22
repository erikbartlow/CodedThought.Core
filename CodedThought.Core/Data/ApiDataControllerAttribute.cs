namespace CodedThought.Core.Data {

	public enum ApiDataControllerOptions {
        /// <summary>Informs the framework that the Api calls with this object do not use a controller and should only use the endpoint plus the action.</summary>
        NoController = 0
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class ApiDataControllerAttribute : Attribute {

		#region Properties

		public string ControllerName { get; set; }
		public string? Action { get; set; }
		public BindingList<ApiDataParameterAttribute> Properties { get; set; }

		public ApiDataParameterAttribute? Key { get; set; }
		public Type? ClassType { get; set; }
		public string? ClassName { get; set; }
		public ApiDataControllerOptions Options { get; set; }

		#endregion Properties

		#region Constructors

		public ApiDataControllerAttribute() {
			ClassName = string.Empty;
			ControllerName = string.Empty;
			Properties = new();
		}
        public ApiDataControllerAttribute(ApiDataControllerOptions options) {
            ClassName = string.Empty;
            ControllerName = string.Empty;
            Properties = new();
			Options = options;
        }
        /// <summary>Initializes this object as an Api capable object with it's controller parameter.</summary>
        /// <param name="controller"></param>
        /// <param name="action">    </param>
        public ApiDataControllerAttribute(string controller) : this() {
			ControllerName = controller;
		}

		/// <summary>Initializes this object as an Api capabile object with it'controller and optional action parameters</summary>
		/// <param name="controller"></param>
		/// <param name="action">    </param>
		public ApiDataControllerAttribute(string controller, string action) : this(controller) {
			Action = action;
		}

		#endregion Constructors
	}
}