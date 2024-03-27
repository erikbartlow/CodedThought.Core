# CodedThought.Core.Data.SqlServer
## _A .NET Core Data Entity Provider for Sql Server._

### Requirements

SqlServer requires CodedThough.Core.Configuration and CodedThought.Core.  These can be found in NuGet.
	- [CodedThought.Core.Configuration](https://www.nuget.org/packages/CodedThought.Core.Configuration)
	- [CodedThought.Core](https://www.nuget.org/packages/CodedThought.Core/)

### Usage
1. Install required packages.  See requirements above.
2. Add the Database Connection settings in the appSetting.json or a custom .json file.
    > Note:  See [CodedThought.Core.Configuration](https://www.nuget.org/packages/CodedThought.Core.Configuration) for JSON configguration specifications.
3. Add a new class and inherit from `CodedThough.Core.Data.GenericDataStoreController`.  Below is a sample class file.
```c sharp
    using CodedThought.Core;
    using CodedThought.Core.Data;
    public class DataController : GenericDataStoreController {

		public DataController(IMemoryCache cache, CodedThought.Core.Configuration.ConnectionSetting	 connectionSetting	) {
			DataStore = new GenericDataStore(cache, connectionSetting, "dbo");
		}

		public Person GetPerson(string email)
		{
			try
			{
				ParameterCollection param = DataStore.CreateParameterCollection();
				param.AddStringParameter("EmailAddress", email);
				return DataStore.Get<Person>(param);
			} catch (Exception)
			{

				throw;
			}
		}
            
        public List<Person> GetPeople() {
            try {
                return DataStore.GetMultiple<Person>(null);
            } catch {
                throw;
            }
        }
    }
    using System.Data;
    using CodedThought.Core;
    using CodedThought.Core.Data;
	///<summary>
	/// Person class used by the framework to locate the right table and columns with class and proprety annotations.
	///</summary>
	[DataTable("tblPeople")]
	public class Person
	{
		public Person() { }
		[DataColumn("EmailAddress", DbType.String, DataColumnOptions.PrimaryKey)]
		public string EmailAddress { get; set; }
        [DataColumn("FirstName", DbType.String)]
		public string FirstName { get; set; }
		[DataColumn("LastName", DbType.String)]
		public string LastName { get; set; }
	}
```

