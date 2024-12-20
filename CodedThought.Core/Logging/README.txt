# CodedThought.Core.Logging
## _A Custom ILogger implementation to log to local files_

### Configuration

The FileLoggerProvider uses the "Logging" section of the appsettings.json configuration file. You just need to add a "FileLogger" section under the "Logging" section.

```cs
{
	"Logging": {
		...,
		"FileLogger": {
			"Options": {
				"FolderPath": "C:\\logs",
				"FileName": "log_{date}.log",
				"LeadingDateFormat": "yyyy-MM-dd HH:mm:ss+00:00"
			},
			"LogLevel": {
				"{Your Namespace Here}": "Information"
			}
		}
	},
	...
}
```
#### Configuration Options
| Option | Description | Default
| ------ | ------ | ------
| _FolderPath_ | The physical path to where the log file(s) will be stored. | None
| _FilePath_ | The name of the log file. You can use {date} to have the provider replace it with the current date/time. | "log_{date}.log"
| _LeadingDateFormat_ | If this option is set, the provider will prepend each log entry with a date matching the format entered. | "yyyy-MM-dd HH:mm:ss+00:00"

#### Implementation

```cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureWebHostDefaults(webBuilder =>
		{
			webBuilder.UseStartup<Startup>();
		})
		.ConfigureLogging((hostBuilderContext, logging) =>
		{
			logging.AddAddFileLogger(options =>
			{
				hostBuilderContext.Configuration.GetSection("Logging").GetSection("FileLogger").GetSection("Options").Bind(options);
			});
		});
```