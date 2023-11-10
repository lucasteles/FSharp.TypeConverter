[![CI](https://github.com/lucasteles/FSharp.TypeConverter/actions/workflows/ci.yml/badge.svg)](https://github.com/lucasteles/FSharp.TypeConverter/actions/workflows/ci.yml)
[![Nuget](https://img.shields.io/nuget/v/FSharp.TypeConverter.svg?style=flat)](https://www.nuget.org/packages/FSharp.TypeConverter)

# FSharp.TypeConverter

## Getting started

[NuGet package](https://www.nuget.org/packages/FSharp.TypeConverter) available:

```ps
$ dotnet add package FSharp.TypeConverter
```

> **💡** You can check a complete sample [HERE](https://github.com/lucasteles/FSharp.TypeConverter/tree/master/BasicApi)

## How to use

### Option<'T>

For `Option<'T>` you will need to call `TypeDescriptor.addDefaultOptionTypes()`, this will add type-converters for
almost all BLC and common types.

If you need to use any other optional type you can add it manually with `TypeDescriptor.addOption<YourType>`

### Discriminated Unions

This library will deal with only two types of union types:

- Single Case Unions (ex: `type Email = Email of string`)
- Fieldless Unions (ex: `type Light = On | Off`)

To add definitions for your type just call `TypeDescriptor` functions at the very beginning of your program:

```fsharp
TypeDescriptor.addUnion<Email>
TypeDescriptor.addUnion<Light> 
```

> **💡**: _This also adds the `Option<T>` handler for
the registered union type_

If you have a lot of types you can scan and load all valid union types
using `TypeDescriptor.addUnionTypesInAssemblyContaining<TypeOnAssembly>`

### Example

Using for ASP.NET
Core [Options Pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
---
> appsettings.json

```json
{
  "Person": {
    "FirstName": "Jonny",
    "LastName": "Cage",
    "Email": "fsharp@dotnet.com",
    "Age": 35,
    "Status": "Enabled",
    "WebSite": "https://fsharpforfunandprofit.com"
  },
  "PersonEmpty": {
    "FirstName": "Goro",
    "Status": "Disabled",
    "Email": "prince@shokan.com",
    "LastName": null
  }
}
```

---
> Program.fs

```fsharp
open System.ComponentModel

// Single case types
type Email = Email of string
type Natural = Natural of int
// Fieldless union
type Status = Enabled | Disabled

// Option model
[<CLIMutable>]
type PersonSettings =
    { FirstName: string
      LastName: string option
      Email: Email
      Age: Natural option
      Status: Status
      WebSite: Uri option }

// register option for BCL and common types
TypeDescriptor.addDefaultOptionTypes ()

// register all single case union types and empty unions on the assembly
TypeDescriptor.addUnionTypesInAssemblyContaining<PersonSettings>

// alternatively you can add one by one with
// TypeDescriptor.addUnion<Email>
// TypeDescriptor.addUnion<Natural>

let builder = WebApplication.CreateBuilder Array.empty

builder.Services
    .AddOptions<PersonSettings>()
    .BindConfiguration("Person")
    .ValidateOnStart()
|> ignore

let app = builder.Build()

// Options from dependency injection
let person = app.Services.GetRequiredService<IOptions<PersonSettings>>()
printfn $"Complete %A{person.Value}"

// Getting direct from the IConfiguration
let configuration = app.Services.GetRequiredService<IConfiguration>()
let emptyPerson = configuration.GetSection("PersonEmpty").Get<PersonSettings>()
printfn $"Missing %A{emptyPerson}"
```
---
> Output
```fsharp
{ FirstName = "Jonny"
  LastName = Some "Cage"
  Email = Email "fsharp@dotnet.com"
  Age = Some (Natural 35)
  Status = Enabled
  WebSite = Some https://fsharpforfunandprofit.com/ }
  
{ FirstName = "Goro"
  LastName = None
  Email = Email "prince@shokan.com"
  Age = None
  Status = Disabled
```