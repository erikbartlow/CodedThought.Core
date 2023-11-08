# CodedThought.Core.Data.ApiServer
## _A .NET Core Data Entity Provider for REST APIs._

### Requirements

ApiServer requires CodedThough.Core.Configuration and  CodedThought.Core.  These can be found in NuGet.
	- [CodedThought.Core.Configuration](https://www.nuget.org/packages/CodedThought.Core.Configuration)
	- [CodedThought.Core](https://www.nuget.org/packages/CodedThought.Core/)

### Usage
1. Install required packages.  See requirements above.
2. Add the Api Connection settings in the appSetting.json or a custom .json file.
    > Note:  See [CodedThought.Core.Configuration](https://www.nuget.org/packages/CodedThought.Core.Configuration) for JSON configguration specifications.
3. Add a new class and inherit from `CodedThough.Core.Data.GenericApiDataStoreController`.  Below is a sample class file.
```c sharp
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    public class AppApiController : GenericApiDataStoreController {

        public AppApiController(IMemoryCache cache, CodedThough.Core.Configuration.ConnectionSetting connectionSetting){
           try{
                DataStore = new GenericApiDataStore(cache, connectionSetting)   
           } catch {
               throw;
           }
        }
            
        public async Task<ApiResponse<Person>> GetPeople(string email) {
            try {
                ParameterCollection param = DataStore.CreateParameterCollection();
                param.AddApiParameter(DataStore.GetApiParameterNameFromProperty<Person>("EmailAddress"), email);
                ApiResponse<Person> response = await DataStore.Get<ApiResponse<Person>, Person>(action: "person", param);
                return response;
            } catch {
                throw;
            }
        }
    }
    ///<summary>
    /// Generic response class to accept the data or value response, any message, 
    /// and status code for the call.
    ////</summary>
    public class ApiResponse<T>{
        public List<T> Data { get; set; }
        public object? Value { get; set; }
        public string? Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    
        public ApiResponse(){
            Data = new List<T>();
            Value = null;
            StatusCode = HttpStatusCode.OK;
        }
        public ApiResponse(List<T> data, string? message){
            Data = data;
            Message = message;
            Value = null;
        }
    } 
```
