# ValidationExpressions
C# Library for expression based data validation.  The validation expression library is designed to provide the designer with a flexible system of expressions to validate incoming data or file contents.  Validation expressions are applied at the file level as well as the column level.

Each validation is comprised of a series of one or more expressions that are applied at the moment of upload.  Expressions configured at the file level are applied initially, followed by each column's expression according to their ordinal position in the file.

The goal of an expression is translate to a TRUE or FALSE statement.  As in any boolean type expression a false found in any part of the expression results in a false for the entire expression.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See Installing for notes on how to deploy the project on a live system.

### Prerequisites

The only dependencies are [CodedThought.Core](https://www.nuget.org/packages/CodedThought.Core/) and [NewtonSoft.Json](https://www.newtonsoft.com/json).

### Installing

Installation is simply done by installing the CodedThought.Core.Validation package.

### Configuration
*Optionally you can set the validation exception messages in a separate .json file.  If you choose this route I suggest using the CodedThought.Core.Configuration extension to add additional json configuration files to the .NET Core builder.  If these are not set then the default constants will be used.*

#### Config File Validation Settings

Add the `CodedThoughtValidationSettings` section in the `appSettings.json` or custom `json` settings file.
  
```json
  CodedThoughtValidationMessages:{
    "Required" : "",
    "Equals" : "",
    "GreaterThan" : "",
    "GreaterThanEqTo" : "",
    "LessThan" : "",
    "LessThanEqTo" : "",
    "NotEqual" : "",
    "InvalidEmail" : "",
    "NotInList" : "",
    "NotBetween" : "",
    "NotUpper" : "",
    "NotLower" : "",
    "ExceedsMax" : "",
    "MinimumNotReached" : ""
  }
```

## Modifier Reference
Every expression can be broken down into two parts.  The target and the modifer.
The example below shows an expression in its most basic form.  All expressions must be surrounded by brackets, and the value, *this*, to the left of the pipe, **|**, is the target of the expression while the **r** is the modifier that will be used to validate the target.
```
[ this | r ]
```

### Numeric Expressions
All standard comparison based expressions are supported such as =, >, <, !=.

### Text Expressions
>Note:  The % symbol is used to evaluate as a "contains".
```
[ %"FRANCE" ]
"" is used to evaluate the contents of the quotes against the contents of the column.
[ ="FRANCE" ] evaluates to only return true if the column value is exactly FRANCE.
[ !="FRANCE" ] evalues to only return true if the column value is NOT exactly FRANCE.
```
### Combining Expressions
##### The true power of this validation engine comes from its ability to combine and next expressions.
Expressions can be string together to accomplish AND and OR conditions.

| AND/OR | Explanation |
|:---:|-|
|**&&**| ``` [="FRANCE"] && [="BELGIUM"]``` |
|**\|\|**| ``` [="FRANCE"] || [="BELGIUM"]``` Applies both expressions with an OR. |
|**Grouping**| ```([="FR"] \|\| [="BE"]) && ([!=""])``` Expressions can be grouped to create more complex expressions.|

### Available Modifiers
| Modifier| Description
|:---:|-
| \| u|**Upper Case**:  Applied to a string expression and the evaluation will compare the column and expressions's upper case values
\| l|**Lower Case**:  Applied to a string expression and the evaluation will compare the column and expression's lower case values.
\| i|**Case Insensitive**:  Applied to a string expression and the evaluation will compare the column and expression's values regardless of their case.
|\| ru|**Round Up**:  Applied to a numeric expression and the evaluation will compare column's rounded value.
|\| rd|**Round Down**:  Applied to a numeric expression and the evaluation will compare column's rounded value.
|\| mx(n)|**Max (n)**: Text Based - When applied to a text value this will validate that the value is not greater than N.  Number Based: When applied to a numeric value this will validate that the value is not greater than N.
|\| mn(n)|**Min (n)**: Min (n): Text Based - When applied to a text value this will validate that the value's length is less than N.  Number Based: When applied to a numeric value this will validate that the value is less than N.
|\| b(n1,n2)|**Between (n1, n2)**: Applied to a numeric expression and the evaluation will apply a **between** comparison using **n1** and **n2**.
|\| in(n1,...)|**In(n1, n2, n3, ...)** : Applied to a Numeric or text based expression and the evaluation will test if the target is one of the values  in the parameter list.
|\| e|**Email**: Applied to a string expression and the evaluation will consider the target to be an email address, and will validate it accordingly.
|\| r|**Required**: Used after or a modifier or used alone this will set the main expression to require a value.  Example:  The following expression, ```[ this | mn(5), r ]```, will require the column being valiated to have a value.  An alternative method is to the minimum modifier set to mn(1) for a text based column.  However, the **r** modifier is more reliable when going between text based and numericly based values.
|\| InDb|**InDB**:  The target must be one of the values in the parameterized list obtained from a database source.
> NOTE: Any string expression, by default, uses this modifier.
### Flags
##### Flags are used as options for the modifiers.  They must be contained within the expression's brackets and must follow the expressions modifier separated by commas.  As of this document's version there is only one flag.
|Flag|Description
|-|-
|, f|**Force**:  Used within a string based expression, this will force the main modifier to apply to the value instead of just validating against it.

Example:  The following expression, ```[ this | u, f ]```, will force the value of this to be in upper case.
> Note:  At this time force is the only flag.  More are planned for a later release.

## Authors

* Erik Bartlow 

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details