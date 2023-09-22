namespace CodedThought.Core.Data {

	public class GenericApiDataStoreController : IDisposable {

		/// <summary>Gets or sets the name of the connection.</summary>
		/// <value>The name of the connection.</value>
		public string ConnectionName { get; set; }

		/// <summary>Gets or sets the data store.</summary>
		/// <value>The data store.</value>
		public GenericApiDataStore DataStore { get; set; }

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose() {
			if (DataStore != null) {
				DataStore = null;
			}
		}
	}
}