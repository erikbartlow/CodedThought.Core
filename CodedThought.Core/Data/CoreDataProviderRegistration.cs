using CodedThought.Core.Data.Interfaces;

namespace CodedThought.Core.Data
{

    /// <summary>
    /// Describes a registered <see cref="IDatabaseObject" /> provider so a new provider instance can be created for each data operation instead of sharing one instance across the process.
    /// </summary>
    public sealed class CoreDataProviderRegistration
    {

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="CoreDataProviderRegistration" /> class.</summary>
        /// <param name="providerType">     The concrete provider type to instantiate.</param>
        /// <param name="supportedDatabase">The database the provider supports.</param>
        /// <param name="key">              The optional key the provider was registered with.</param>
        public CoreDataProviderRegistration(Type providerType, DBSupported supportedDatabase, object? key)
        {
            ProviderType = providerType;
            SupportedDatabase = supportedDatabase;
            Key = key;
        }

        #endregion Constructors

        #region Properties

        /// <summary>Gets the concrete provider type to instantiate.</summary>
        public Type ProviderType { get; }

        /// <summary>Gets the database the provider supports.</summary>
        public DBSupported SupportedDatabase { get; }

        /// <summary>Gets the key the provider was registered with, or <c>null</c> when it was registered without one.</summary>
        public object? Key { get; }

        #endregion Properties
    }
}
