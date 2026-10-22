# CodedThought.Core.Configuration

## A .NET Core Configuration manager to use with the CodedThought.Core framework or alone with any other .NET Core project

Usage of the CodedThought.Core.Configuration library is done with a few extension methods.

### Installation and Usage

1. Reference the package in Nuget.
2. To add a reference to the standard .NET appSettings.json simply call the `AddAppSettingsConfiguration` extension method off of the builder class.
3. To add an additional set of custom settings outside of the appSettings.json file.
    1. Create your own .json configuration file alongside the appsettings.json file. It should follow the appsettings.json JSON format.
    2. Add the `AddCoreSettingsConfiguration` extension method off of the app builder. You may choose to use the default ctSettings.json file name or provide your own.
4. All methods support passing in the current environment to enable environment specific settings.
5. Register the settings with `AddCoreSettings` so the section is bound once and validated when the host starts.

```cs
builder.Configuration.AddCoreSettingsConfiguration(env: builder.Environment, optional: false, reloadOnChange: true);
builder.Services.AddCoreSettings(builder.Configuration);
```

### Configuration Sources

`AddCoreSettingsConfiguration` adds, in order, the settings file, the optional `ctSettings.{Environment}.json` overlay, and the environment variables. Pass `includeEnvironmentVariables: false` to omit the last source. The `optional` and `reloadOnChange` arguments are applied to the base settings file; the environment overlay is always optional.

### Connection Accessors

| Method | Description &amp; Remarks
| ------ | ------
| _GetCoreSettings()_ | Binds the CoreSettings section into a new instance.
| _GetCorePrimaryConnection()_ | Returns the connection marked `Primary=true`, or `null` when none is marked.
| _GetDatabaseConnection(name)_ | Returns the connection matching the name, or `null` when absent. Matching ignores case.
| _GetRequiredCorePrimaryConnection()_ | Returns the primary connection, throwing an InvalidOperationException that lists the configured connections when none is marked.
| _GetRequiredDatabaseConnection(name)_ | Returns the named connection, throwing an InvalidOperationException that lists the configured connections when absent.
| _GetCoreDatabaseProviders()_ | Returns the distinct ProviderType values across all configured connections.

### Startup Validation

`CoreSettingsValidator` runs through `ValidateOnStart()` and reports every failure at once. It rejects a section with no connections, a connection count of `Primary=true` other than exactly one, duplicate connection names, a blank Name or ConnectionString, a negative Timeout, and any ProviderType outside SqlServer, Oracle, MongoDb, OleDb, MySql, ApiServer, and PostgreSQL.

### Release Notes

| Feature | Description &amp; Remarks
| ------ | ------
| Options Pattern | _AddCoreSettings(IConfiguration)_ binds the CoreSettings section through _IOptions&lt;CoreSettings&gt;_ and validates it with _ValidateOnStart()_. Inject _IOptionsMonitor&lt;CoreSettings&gt;_ to observe settings-file changes, or _IOptionsSnapshot&lt;CoreSettings&gt;_ for a value fixed for the lifetime of a request.
| Required Connection Accessors | **Breaking.** _GetCorePrimaryConnection_ and _GetDatabaseConnection_ now return a nullable ConnectionSetting, matching their actual behavior. Use the _GetRequired*_ companions where a missing connection should throw.
| Environment Variable Overlay | The _AddCoreSettingsConfiguration_ overloads now honor their _optional_ and _reloadOnChange_ arguments, always add the optional _ctSettings.{Environment}.json_ overlay, and append the environment variable provider by default so secrets can be supplied from the environment.
| Case-Insensitive Settings | _CoreSettings.Settings_ is matched without regard to case through the new _TryGetSetting_ and _GetSetting_ methods. _ApplicationCookieName_ resolves regardless of its position in the file.
| WINFORM Setting Removed | **Breaking.** The WINFORM setting and the _CoreSettings.IsWinForm_ property are removed.
| Validation Messages | **Breaking.** _ValidationConfigurationProvider_ implements the new _IValidationMessageProvider_ instead of _IConfigurationProvider_. Register it with _AddCoreValidationMessages(IConfiguration)_. Absent or blank keys fall back to the built-in defaults.
| CoreSettingsRoot Removed | **Breaking.** The unused _CoreSettingsRoot_ wrapper and the _ValidationMessageElement_ type are removed.
| Provider Validation Fixed | _ConnectionSetting.CheckProvider()_ no longer throws for valid providers. Supported types are exposed through _ConnectionSetting.SupportedProviders_.
| Dependency Cleanup | The Newtonsoft.Json reference is removed and _Microsoft.Extensions.Hosting_ is narrowed to _Hosting.Abstractions_. Package versions are managed centrally from the root _Directory.Packages.props_.