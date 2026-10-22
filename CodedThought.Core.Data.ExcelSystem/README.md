# CodedThought.Core.Data.ExcelSystem

## _A .NET Core Excel and CSV reader and writer for CodedThought.Core._

### Requirements

- [CodedThought.Core](https://www.nuget.org/packages/CodedThought.Core/)
- [DocumentFormat.OpenXml](https://www.nuget.org/packages/DocumentFormat.OpenXml/) 3.3.0

The package targets `net10.0`.

> Note: Unlike the database providers, this package is not a `GenericDataStore` provider. It has no
> connection string, no `DatabaseType` setting and no dependency on
> `CodedThought.Core.Configuration`. It reads and writes files directly.

### What Is In The Package

| Type | Namespace | Purpose |
| ------ | ------ | ------ |
| `ExcelUtility` | `CodedThought.Core.Data.ExcelSystem` | Opens `.xls`, `.xlsx`, `.xlsm` and delimited text files as a `DataSet`, and converts a `List<T>` to a `DataTable`. |
| `ExcelOpenXmlWriter` | `CodedThought.Core.Data.ExcelSystem` | Writes a `DataSet`, `DataTable` or `List<T>` to an `.xlsx` file or stream. |
| `ExcelReaderFactory` | `CodedThought.Core.Data.ExcelSystem` | Creates an `IExcelDataReader` over a stream, either binary (`.xls`) or Open XML (`.xlsx`). |
| `IExcelDataReader` | `CodedThought.Core.Data.ExcelSystem` | An `IDataReader` over a workbook, adding `AsDataSet` and `IsFirstRowAsColumnNames`. |
| `ExcelColumnAttribute` | `CodedThought.Core.Data.ExcelSystem` | Maps a property to a worksheet column by name, order and format. |
| `Extensions` | `CodedThought.Core.Extensions` | The `ToListFromExcel<T>` and `GetExcelColumnAttributes` extension methods. |
| `ExcelWriter` | `CodedThought.Core.Data.ExcelSystem` | Obsolete BIFF writer. Use `ExcelOpenXmlWriter` instead. |

### Usage

1. Install the required packages. See requirements above.
2. Annotate the properties of a data aware class with `ExcelColumnAttribute`.

> Note: See the CodedThought.Core package README for more on data aware classes.
> `ToListFromExcel<T>` only populates a property that carries **both** an `ExcelColumn` and a
> `DataColumn` attribute, because it resolves the target type from `DataColumnAttribute.ColumnType`.

```csharp
using System.Data;
using CodedThought.Core.Data;
using CodedThought.Core.Data.ExcelSystem;

namespace Sample.Data {

    [DataTable("COUNTRY", "", "dbo")]
    public class Country {

        [ExcelColumn("Country Name", 0)]
        [DataColumn("CountryName", DbType.String, DataColumnOptions.PrimaryKey)]
        public string CountryName { get; set; }

        [ExcelColumn("Region Code", 1)]
        [DataColumn("RegionCode", DbType.String)]
        public string RegionCode { get; set; }

        [ExcelColumn("ISO Code", 2)]
        [DataColumn("CountryCodeISO", DbType.String)]
        public string ISOCode { get; set; }

        [ExcelColumn("Population", 3, typeof(int), "0", ExcelColumnAttribute.ExcelStyleFormats.Numeric)]
        [DataColumn("Population", DbType.Int32)]
        public int Population { get; set; }

        [ExcelColumn(true)]
        public string InternalNotes { get; set; }
    }
}
```

#### ExcelColumnAttribute Members

| Member | Description |
| ------ | ------ |
| `ColumnName` | The worksheet column heading. Also used to match a header row back to a property. |
| `ColumnOrder` | The zero based column position. Used when reading rows back into objects. |
| `ColumnType` | The CLR type of the column. Defaults to `typeof(string)`. |
| `NullDefaultValue` | Written in place of `null`. |
| `Format` | A format string, or one of the `ExcelStyleFormats` values. |
| `Bold` | Renders the column bold. |
| `ColumnAlignment` | A `DocumentFormat.OpenXml.Drawing.Wordprocessing.HorizontalAlignmentValues` value. Defaults to `Left`. |
| `Ignore` | Excludes the property. Set by the `ExcelColumnAttribute(bool)` constructor. |

`ExcelStyleFormats` carries the built in Excel format identifiers: `NoStyle`, `Numeric`,
`NumericTwoDecimals`, `NumericTenths`, `NumericTenthsTwoDecimals`, `Percentage`,
`PercentageTwoDecimals`, `DateMMDDYYYY`, `DateDMMMYYYY`, `DateMMMYYYY` and `DateMDYYYHMM`.

### Writing A Workbook

`ExcelOpenXmlWriter` writes Open XML (`.xlsx`). Every method is static.

```csharp
// To a file.
ExcelOpenXmlWriter.CreateExcelDocument(countries, @"C:\Exports\Countries.xlsx");
ExcelOpenXmlWriter.CreateExcelDocument(dataTable, @"C:\Exports\Countries.xlsx");
ExcelOpenXmlWriter.CreateExcelDocument(dataSet, @"C:\Exports\Countries.xlsx");

// To a stream, for example to return from an API action.
string fileName = $"ExcelOutput_{DateTime.Today:yyyyMMddHHmmss}.xlsx";
Stream output = ExcelOpenXmlWriter.CreateExcelDocumentAsStream(model.Countries, fileName);
FileContentResult fcr = new(_common.ConvertToByteArray(output),
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
    FileDownloadName = fileName
};
```

One worksheet is created per `DataTable` in the `DataSet`, and `DataTable.TableName` becomes the
worksheet name. Set `TableName` before writing, because a `DataTable` built from a list has an empty
name. The first row is always written as a header row from the column names.

> Note: `ExcelOpenXmlWriter` has its own `ListToDataTable<T>` that reflects over every public
> property and does not read `ExcelColumnAttribute`. To apply the attribute metadata, build the
> `DataTable` with `ExcelUtility.ListToDataTable<T>` and pass that instead of the list.

### Reading A Workbook

`ExcelUtility.OpenExcelFileAsDataSet` selects the reader from the file extension, `.xls` for the
binary reader and `.xlsx` or `.xlsm` for the Open XML reader. The extension must include the leading
period. Failures are raised as `CodedThoughtException`.

```csharp
DataSet ds = ExcelUtility.OpenExcelFileAsDataSet(@"C:\Imports\Countries.xlsx", hasHeaders: true);
IList<Country> countries = ds.Tables[0].ToListFromExcel<Country>();
```

For direct control over the reader, use `ExcelReaderFactory`.

```csharp
using Stream stream = File.Open(path, FileMode.Open, FileAccess.Read);
using IExcelDataReader reader = ExcelReaderFactory.CreateBinaryReader(stream, ReadOption.Loose);
reader.IsFirstRowAsColumnNames = true;
DataSet ds = reader.AsDataSet(convertOADateTime: true, hasHeaders: true);
```

`CreateBinaryReader` accepts a `ReadOption` of `Strict` or `Loose`, and a `convertOADate` flag.
`CreateOpenXmlReader` takes the stream only. Check `IsValid` and `ExceptionMessage` on the reader
when a read returns no results.

### Reading A Delimited File

`OpenCSVFileAsDataSet` reads any single or multi character delimiter. The non generic overloads type
every column as `string`. The generic overloads resolve each column's type from `T`, matching header
names to `ExcelColumnAttribute.ColumnName` when `hasHeaders` is `true`, and falling back to property
order when it is `false`.

```csharp
using Stream stream = File.Open(path, FileMode.Open, FileAccess.Read);
DataSet ds = ExcelUtility.OpenCSVFileAsDataSet<Country>(stream, ",", hasHeaders: true);
```

When a file has no header row, pass `hasHeaders` and `columnCount` so the column set can be sized.
Both arguments are required on the four parameter overloads.

```csharp
DataSet ds = ExcelUtility.OpenCSVFileAsDataSet<Country>(stream, ",", false, 4);
```

### File Encoding

Delimited files are read with `ExcelUtility.FileEncodingToUse`, which is driven by the static
`ExcelUtility.CurrentFileEncoding` property. Set it before reading.

```csharp
ExcelUtility.CurrentFileEncoding = ExcelUtility.EncodingToUse.ISO;
```

`EncodingToUse` supports `UTF8` (the default), `UTF7` and `ISO`, where `ISO` maps to `ISO-8859-1`.

> Note: `CurrentFileEncoding` is static and therefore process wide. It is not safe to change it
> concurrently from multiple requests.

### Behavior And Limitations

| Area | Behavior |
| ------ | ------ |
| Write format | `ExcelOpenXmlWriter` emits `.xlsx` only. `ExcelWriter` writes the legacy BIFF format and is marked `[Obsolete]`. |
| Cell typing on write | A column is written as numeric only when its `DataType` is `System.Decimal` or `System.Int32`. Every other type, including `DateTime`, is written as text. |
| Styling on write | `ExcelOpenXmlWriter` adds an empty `Stylesheet`, so `Format`, `Bold` and `ColumnAlignment` from `ExcelColumnAttribute` are not applied to the output file. |
| Reading into objects | `ToListFromExcel<T>` reads cells by `ColumnOrder` in both the `hasHeaders` `true` and `false` cases. A parse failure throws and includes the offending row. |
| `.csv` through the Excel reader | `OpenExcelFileAsDataSet` recognizes `.xls`, `.xlsx` and `.xlsm` only. Use `OpenCSVFileAsDataSet` for delimited text. |
| Exceptions | `ExcelUtility` wraps failures in `CodedThought.Core.Exceptions.CodedThoughtException`. The CSV methods throw a plain `Exception` carrying the failing row number. |

## Release Notes

### 10.0.0-alpha-1

| Feature | Description &amp; Remarks |
| ------ | ------ |
| Upgraded to .NET 10 | The project targets `net10.0` with `ImplicitUsings` and `Nullable` enabled, and references `CodedThought.Core` through a `ProjectReference`. |
| Centrally managed dependencies | `DocumentFormat.OpenXml` and the remaining package references take their versions from `Directory.Packages.props`. |
| Fixed `OpenExcelFileAsDataSet(StreamReader, string, bool)` | The overload called itself and recursed until the stack was exhausted. It now forwards `StreamReader.BaseStream` to the `Stream` overload. |
| Fixed `OpenExcelFileAsDataSet(string, bool)` | The file path overload opened a `FileStream` that it neither used nor closed, then read the workbook through a `StreamReader`. It now opens one `FileStream` inside a `using` and passes it to the `Stream` overload. |
| **Breaking.** `columnCount` is now required | `OpenCSVFileAsDataSet(Stream, string, bool, int)` and `OpenCSVFileAsDataSet<T>(Stream, string, bool, int)` no longer default `hasHeaders` or `columnCount`. The defaults made these overloads ambiguous with the three parameter forms on any two argument call. A call that already passed four arguments is unaffected. |
| Fixed `OpenCSVFileAsDataSet<T>(Stream, string, bool, int)` | When `hasHeaders` was `true` the method delegated to the non generic overload, returning `string` typed columns instead of columns typed from `T`. It now delegates to the generic overload. The `columnCount` argument was also overwritten with the property count of `T` before being validated; it is now honored and checked against `T`. |