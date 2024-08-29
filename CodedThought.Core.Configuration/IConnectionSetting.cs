namespace CodedThought.Core.Configuration
{
    public interface IConnectionSetting
    {
        string ConnectionString { get; set; }
        string DefaultSchema { get; set; }
        string Name { get; set; }
        bool Primary { get; set; }
        string ProviderName { get; set; }
        string ProviderType { get; set; }
        int Timeout { get; set; }

        void CheckProvider();
        void CheckProvider(string providerType);
    }
}