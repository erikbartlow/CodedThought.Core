# CodedThought.Core.Configuration
## A .NET Core Configuration manager to use with the CodedThought.Core framework or alone with any other .NET Core project.

Usage of the CodedThought.Core.Configuration libarary is done with a few extension methods.

###Installation and Usage

1. Reference the package in Nuget.
2. To add a reference to the standard .NET appSettings.json simply call the AddAppSettingsConfiguration extension method off of the builder class.
3. To add an additional set of custom settings outside of the appSettings.json file.
	a.  Create your own .json configuration file alongside the appsettings.json file.  It should follow the appsettings.json JSON format.
	b.  Add the AddCoreSettingsConfiguration extension method off of the app builder.  You may choose to use the default ctSettings.json file name or provider your own.
4. All methods support passing in the current environment to enable environment specific settings.