# CodedThough.Core
## _A Custom Entity Framework and .NET Utility Codebase_

The CodedThought.Core library is a custom entity framework used to primarily abstract the database away from consuming components or clients.  It has many other useful aspects:
* Sql Server, Oracle, and REST Api Data Providers
* Email and Address Parsing
* Excel Import and Generation
* Extensions
* LDAP API Integrations
* SFTP and cURL Integration
* ASP.NET WebControls
* US Holiday Calculator
* Validation Engine with highly customizable expression based language

### Installation

CodedThought.Core requires several package dependencies, but these are easly done using the existing NuGet packages referenced.Installation is simply done by referencing the CodedThought.Core and Configuraion libraries and any of the source specific Core Provider libraries like CodedThought.Core.Data.SqlServer.

A recommended approach to installation is by using the nuget package manager.  The path to all the packages is https://nuget.pkg.github.com/erikbartlow/index.json.
## Usage

Application Settings are accessed via .NET Core appsettings.json while database and/or API connection details are stored in a custom json settings file named ctSettings.json.  CodedThought.Core supports environment based settings.json implementations.


##### ctsettings

### Core Settings Parameters
### CoreSettings/Settings
|Property | Options | Description |
| ------ | ------ | ------
| WINFORM | `true | false` | The purpose of this setting is to tell the CodedThought.Core framework if it is being used in a web environment or not.  The reason for this is so that the framework can cache the discovered data aware classes and associated types properly.
|ApplicationCookieName|`Name of root application cookie`|Provide a custom name for your application cookie.
### Connection Parameters
### CoreSettings/Connections
The connections settings is an array of CoreSettings/Connections.  However, only one can have the `Primary` key set to `true|false`.
| Property | Options
| ------ | ------
| Name | `string`  Enter a unique name for the connection here.  The CodedThought.Core framework supports multiple database connections on multiple platforms. |
| Primary | `true|false`  Specifies which connection the framework should use by default. |
| ProviderType | Specifies which client provider you are expecting to use with this connection.  Accepted provider types: `[ SqlServer | Oracle | MySql | OleDb | ApiServer ]`.  SqlServer is the default provider type.|
| DefaultSchema | `string` Enter the default schema to use with this connection.  **Note:  This feature allows the developer to create mulitple connections to the same database but pointing to different schemas within the database.  This useful when setting the default schema for a database user is not possible.**`--The ApiServer provider does not require this` |
| ConnectionString | Enter the full connection string here for the database.  For Oracle connections you can enter the full TNS Names entry if necessary or just the TNSNAMES.ora entry name.  For Api connection use the Url endpoint. |
| PoviderName | Enter the `.NET System.Data` allowed provider name for this connection.  This is not a substitute for the `ProviderType` setting, and is only required for as long as your .NET connection requires it.  The ApiServer provider does not require this.  `Microsoft.Data.SqlClient` is the default provider name.|

## Data Entity Management
### Custom Attributes
There are two custom attributes used by `CodedThought.Core.Data`.
* DataTable
* DataColumn
* ApiDataController
* ApiDataParameter

#### DataTable Attribute Properties
| Name | Description |Options & Remarks |
| ------ | ------| ------|
| _TableName_ | `GET|SET` Physical name of the table to get, save, or delete from. |
| _ViewName_ | `GET|SET` Physical name of a view to get from.<br />_Note: This is useful for developers when you have a particular set of data elements that need to be joined to display or report on._ |
|_SchemaName_| `GET|SET` Name of the database schema this entity resides in| `Default is dbo`|
|_SourceName_|`GET` Gets the name of the source based on the `UseView` property and availability of the table and view name properties.|
|_Properties_| `GET` A list of all bound properties in the entity||
|_Key_|`GET` The specific `DataColumnAttribute` current set as the database key| The system currently only supports a single property to be a key and does not support.  **Multiple keys planned for a later release**
##### Sample Usage
```cs
[DataTable( "tblRegions" )]
[DataTable( "tblRegions", "vRegions" )]
```
#### DataColumn Attribute Properties
| Name | Description &amp; Options | Required 
| ------ | ------ | ------
| _ColumnName_ | Physical name of the column in the table to get, save, or delete from. | Yes |
| _IsPrimaryKey_ | Is this property/column the primary key in the table? `true | false` <br />_Default: false_  |  Yes |
| _IsUpdateable_ | Is this property/column considered updateable?  This is usually true unless it is also the primary key. `true | false` <br />_Default: true_  |  No
| _Size_ | Denotes the maximum size of data stored in the column.<br />_Note: This is only used for string based columns._ | No |
| _ColumnType_ | Specifies the `System.Data.DbType` associated with the column.| Yes |
| _PropertyType_ | Used by bulk copy procedures to dynamically ascertain the class to table column type mappings.|No|
| _PropertyName_ | Used by bulk copy procedures to dynamically ascertain the class to table column name mappings.|No|
| _ExtendedPropertyType_ | When setting this to another classes type the developer can set a property that is of an object type rather than a simple type.  This is very useful when a class property references an `ENUM`.<br />_Usage: typeof(MyNamespace.MyEnum)_|No|
| _ExtendedPropertyName_ | The property containing the data for the column.<br />_Note: Only used when the `ExtendedPropertyType` is set and is of type `Object`| No|
|_LoadExtendedPropertyObject_|`true|false` **Not Implemented**|No
|_OverridesInherited_|`true|false`, When set to `true` causes the framework to prefer the current DataColumnAttribute over inherited properties.|No
|_AllowNulls_|`Not Implemented`
##### Sample Usage
```cs
// Constructor Samples
[DataColumn( name: "Id", type: System.Data.Int32, isKey: true )]
[DataColumn( name: "Name", type: System.Data.String, size: 20 )]
[DataColumn( name: "Name", type: System.Data.String, extendedPropertyType: typeof(Country), "CountryName" )]

```

> IMPORTANT: Setting the __`ViewName`__ property will override any `TableName` value previously set for all GET routines.  However, Save and Delete routines will always use the `TableName` value since updateable views are not always supported across database platforms.
#### ApiDataController Attribute Properties
| Name | Description |Options & Remarks |
| ------ | ------ | ------
|_ControllerName_|`string`:  The endpoint controller name where the entity can be reached.|No
|_Action_|`string`: The action name.|Yes
|_Properties_|`GET|SET ApiDataPrameters`: List of all bound action parameters found for the entity.|No
|_Key_| `GET|SET true|false`: Gets or sets the entity's key property.  This is used during Api calls that typicall have an `id=?` parameter.|No
|_ClassType_| `GET|SET typeof()` 
|_ClassName_| `GET|SET string`:  Nullable full namespace for the entity
##### Create Data Aware classes
A key component to the CodedThought.Core.Data framework are the custom tags designed to enable the framework to "learn" how to communicate with your database.  You can design them like any C# class, but it is key to understand that the framework is dynamically creating parameterized queries based on the custom attributes you set.


##### Create a Controller Class that inherits from the `GenericDataStoreController` with at least one constructor.
> Important: The GenericDataStoreController class provides your controller with all the necessary methods for CRUD through the `DataStore` object.  The `DataStore` is the derived instance of the `DatabaseObject`.
```cs
public class DBController : GenericDataStoreController {
    
    public DbController(IMemoryCache memoryCache, CoreConnectionString connectionString) {
      // Connection to database.
      DatabaseConnection cn = new DatabaseConnection( *Connection Name* );
      try {
        DataStore = new GenericDataStore( memoryCache, new(connectionString) );
      } catch ( Exception ex ) {
        throw;
      }
    }

}
```

##### Create a class with data aware properties.
```cs
using System.Data;
using CodedThought.Core.Data;

[DataTable( "tblRegion" )]
public class Region{

	[DataColumn( name: "regionId", type: System.Data.Int32, isKey: true )]
    public int Id{ get; set; }
    [DataColumn( name: "regionName", type: System.Data.String, size: 20 )]
    public string Name { get; set; }
    
}
```
##### Add CRUD Methods to the Controller class
```cs
public Region GetRegion( int id ){
	try{
    	return DataStore.Get<Region>( id );
    } catch {
    	throw;
	}
}

public List<Region> GetRegions(){
	try{
    	// The GetMultiple method requires a ParameterCollection.  So to simply
        // return all records we just have to pass a null ParameterCollection.
    	return DataStore.GetMultiple<Region>(null);
    } catch {
    
    }
}
```
##### ParameterCollection
The ParameterCollection inherits from CollectionBase and implements the IList, ICollection, IEnumerable interface.

| Methods | Description &amp; Options
| ------ | ------
| _Add_ | Adds a parameter to the collection.
| | __Overloads__: <ul><li>( IDataParameterCollection parameters )</li><li>( ParameterCollection parameters )</li><li>( IDataParameter parameter )</li></ul>
| _AddBooleanParameter()_ | `string` Column Name, `boolean` Value
| _AddCharParameter()_ | `string` Column Name, `string` Value, `int` Size
| _AddDataTimeParameter()_ | `string` Column Name, `DateTime` Value
| _AddDoubleParameter()_ |`string` Column Name, `double` Value
| _AddInt32Parameter()_ | `string` Column Name, `int` Value
| _AddOutputParameter()_ | `string` Parameter Name, `DBTypeSupported` Return Value
| _AddStringParameter()_ | `string` Column Name, `string` Value
| _AddSubGroupParameterList()_ | `ParameterCollection` list
| _AddXmlParameter()_ | `string` Column Name, `string` Value
| _Remove()_ | `int` index
| _SubParameterGroupWhereType_ | Using the `DatabaseObject.WhereType` enumerator this instructs the framework on how to handle any sub parameter groups if found.<br /> _Note: Only used when the `AddSubGroupParameterList()` method is used._
#### Load and Access appSettings.json and ctSettings.json
Using the configuration specific extension routines you can auto load the appSettings and any environment based json configurations.
>Note:  The name of the environment is used to locate any environment specific versions of the file. For example, if your environment is name "dev" then your settings file should be appsettings.dev.json.
```cs
builder.AddAppSettingsConfiguration(Optional: IConfigurationBuilder.Environment)
builder.AddCoreSettingsConfiguration(Optional: IHostEnvironment.Environment)
