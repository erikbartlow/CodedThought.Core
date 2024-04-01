# CodedThought.Core.Data.ExcelSystem
## A .NET Core Excel Read/Write Library

### Installation

1. Reference the CodedThought.Core and CodedThought.Core.Configuration packages.
2. Reference the CodedThought.Core.Data.ExcelSystem package.

### Usage

#### Set Up Data Aware Classes
>Note:  Please see the CodedThought.Core package README for more on using data aware classes.
```
// This example is a data aware class for a country object.
namespace Sample.Data {

    [ApiDataController("lookup")]
    [DataTable("COUNTRY", "", "dbo")]
    public class Country {

        #region Constructors

        public Country() {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the country.
        /// </summary>
        /// <value>
        /// The name of the country.
        /// </value>
        [DataColumn("CountryName", DbType.String, DataColumnOptions.PrimaryKey)]
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the region code.
        /// </summary>
        /// <value>
        /// The region code.
        /// </value>
        [DataColumn("RegionCode", DbType.String)]
        public string RegionCode { get; set; }

        /// <summary>
        /// Gets or sets the iso code.
        /// </summary>
        /// <value>
        /// The iso code.
        /// </value>
        [DataColumn("CountryCodeISO", DbType.String)]
        public string ISOCode { get; set; }

        /// <summary>
        /// Gets or sets the iso code3.
        /// </summary>
        /// <value>
        /// The iso code3.
        /// </value>
        [DataColumn("CountryCodeISO3", DbType.String)]
        public string ISOCode3 { get; set; }

        #endregion Properties
    }
}
```
#### Sample Code
##### This is a quick sample for creating an excel file based on the country data aware class above and returning it to an API content result

```
PropertyInfo[] ColumnNames = typeof(Country).GetProperties();
string fileName = $"ExcelOutput_{DateTime.Today:yyyyMMddHHmmss}.xlsx";
Stream output = ExcelOpenXmlWriter.CreateExcelDocumentAsStream(model.Countries, fileName);
FileContentResult fcr = new(_common.ConvertToByteArray(output), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
    FileDownloadName = fileName
};
```