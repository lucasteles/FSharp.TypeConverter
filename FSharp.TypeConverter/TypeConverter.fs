namespace FSharp.TypeConverter

open System
open System.ComponentModel
open Microsoft.FSharp.Reflection

type SingleUnionTypeConverter<'u>() =
    inherit TypeConverter()

    let backingConverter =
        let case = FSharpType.GetUnionCases(typeof<'u>) |> Array.tryExactlyOne

        case
        |> Option.map (fun c -> c.GetFields())
        |> Option.bind Array.tryExactlyOne
        |> Option.map (fun f -> TypeDescriptor.GetConverter f.PropertyType)
        |> Option.zip case

    override this.CanConvertFrom(context, sourceType) =
        backingConverter
        |> Option.exists (fun (_, c) -> c.CanConvertFrom(context, sourceType))

    override this.CanConvertTo(context, destType) =
        backingConverter
        |> Option.exists (fun (_, c) -> c.CanConvertTo(context, destType))

    override this.ConvertFrom(context, culture, value) =
        match backingConverter with
        | None -> null
        | Some(u, c) -> FSharpValue.MakeUnion(u, [| c.ConvertFrom(context, culture, value) |])

    override this.ConvertTo(context, culture, value, destinationType) =
        match backingConverter with
        | None -> null
        | Some(u, c) ->
            let value = FSharpValue.GetUnionFields(value, u.DeclaringType) |> snd |> Array.head
            c.ConvertTo(context, culture, value, destinationType)

type SimpleUnionTypeConverter<'u>() =
    inherit TypeConverter()

    let cases =
        FSharpType.GetUnionCases(typeof<'u>)
        |> Array.filter (fun c -> c.GetFields() |> Array.isEmpty)

    let isValid = cases |> (not << Array.isEmpty)

    override this.CanConvertFrom(context, sourceType) = isValid && sourceType = typeof<string>

    override this.CanConvertTo(context, destinationType) =
        isValid && destinationType = typeof<string>

    override this.ConvertFrom(context, culture, value) =
        cases
        |> Array.tryFind (fun c -> c.Name = string value)
        |> Option.map (fun c -> FSharpValue.MakeUnion(c, [||]))
        |> Option.toObj

    override this.ConvertTo(context, culture, value, destinationType) =
        cases
        |> Array.tryFind value.Equals
        |> Option.map (fun c -> box c.Name)
        |> Option.toObj

module OptionTypeConverter =
    let createOptionType typeParam =
        typedefof<_ option>.GetGenericTypeDefinition().MakeGenericType([| typeParam |])

    let getUnionCases underlyingType =
        let optionType = createOptionType underlyingType

        let cases =
            FSharpType.GetUnionCases optionType
            |> Array.partition (fun x -> x.Name = nameof Some)

        let someCase = fst cases |> Array.exactlyOne
        let noneCase = snd cases |> Array.exactlyOne
        someCase, noneCase

    let isOptionType (t: Type) =
        t.IsGenericType
        && FSharpType.IsUnion t
        && t.GetGenericTypeDefinition() = typedefof<_ option>

    let getUnderlyingType (t: Type) =
        if isOptionType t then t.GetGenericArguments()[0] else null

    let makeOptionValue tValue (value: obj) =
        let someCase, noneCase = getUnionCases tValue

        let relevantCase, args =
            match value with
            | null -> noneCase, [||]
            | :? string as v when String.IsNullOrEmpty v -> noneCase, [||]
            | _ -> someCase, [| value |]

        FSharpValue.MakeUnion(relevantCase, args)

    let extractOptionValue (value: obj) =
        if isNull value then
            null
        else
            let values = FSharpValue.GetUnionFields(value, value.GetType()) |> snd
            if values.Length = 1 then values[0] else null

type OptionTypeConverter<'t>() =
    inherit TypeConverter()

    let baseConverter = TypeDescriptor.GetConverter typeof<'t>

    override this.CanConvertFrom(context, sourceType) =
        baseConverter.CanConvertFrom(context, sourceType)

    override this.CanConvertTo(context, destinationType) =
        baseConverter.CanConvertTo(context, destinationType)

    override this.ConvertFrom(context, culture, value) =
        baseConverter.ConvertFrom(context, culture, value)
        |> OptionTypeConverter.makeOptionValue typeof<'t>

    override this.ConvertTo(context, culture, value, destinationType) =
        match OptionTypeConverter.extractOptionValue value with
        | null -> null
        | fieldValue -> baseConverter.ConvertTo(context, culture, fieldValue, destinationType)
