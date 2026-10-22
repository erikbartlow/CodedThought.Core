namespace CodedThought.Core.Data {

	public class GenericDataStoreController : IDisposable {

		/// <summary>Gets or sets the name of the connection.</summary>
		/// <value>The name of the connection.</value>
		public string ConnectionName { get; set; }

		/// <summary>Gets or sets the data store.</summary>
		/// <value>The data store.</value>
		public GenericDataStore DataStore { get; set; }

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose() {
			if (DataStore != null) {
				// Disposing the store releases the provider and its connection.
				DataStore.Dispose();
				DataStore = null;
			}
			GC.SuppressFinalize(this);
		}

		/// <summary>Tests the database connection.</summary>
		/// <returns></returns>
		public bool TestConnection() {
			try {
				return DataStore.TestConnection();
			} catch (CodedThoughtApplicationException ctEx) {
				throw ctEx;
			}
		}
	}
}