open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open System.ComponentModel

// Single case types
type Email = Email of string
type Natural = Natural of int

// Fieldless union
type Status =
    | Enabled
    | Disabled

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
    // .BindConfiguration("PersonEmpty")
    .ValidateOnStart()
|> ignore

let app = builder.Build()

// Options from dependency injection
let person = app.Services.GetRequiredService<IOptions<PersonSettings>>()
printfn $"%A{person.Value}"

// Getting direct from the IConfiguration
let configuration = app.Services.GetRequiredService<IConfiguration>()
let emptyPerson = configuration.GetSection("PersonEmpty").Get<PersonSettings>()
printfn $"%A{emptyPerson}"
