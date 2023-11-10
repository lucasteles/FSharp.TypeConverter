module System.ComponentModel.TypeDescriptor

open System
open System.Reflection
open FSharp.TypeConverter
open Microsoft.FSharp.Reflection

let private addGenericConverter<'converter when 'converter :> TypeConverter> (convType: Type) (clrType: Type) =
    let converter = typedefof<'converter>.MakeGenericType convType

    TypeDescriptor.AddAttributes(clrType, TypeConverterAttribute converter)
    |> ignore

let addOption<'t> =
    let attr = TypeConverterAttribute typeof<OptionTypeConverter<'t>>
    TypeDescriptor.AddAttributes(typeof<'t option>, attr) |> ignore

let addOptionType t =
    OptionTypeConverter.createOptionType t
    |> addGenericConverter<OptionTypeConverter<_>> t

let addUnionType t =
    let cases = FSharpType.GetUnionCases(t)

    if cases |> Array.forall (fun c -> c.GetFields().Length = 0) then
        addGenericConverter<SimpleUnionTypeConverter<_>> t t
        addOptionType t

    if cases.Length = 1 then
        addGenericConverter<SingleUnionTypeConverter<_>> t t
        addOptionType t

let addUnionTypesInAssembly (assembly: Assembly) =
    assembly.GetTypes()
    |> Array.filter FSharpType.IsUnion
    |> Array.iter addUnionType

let addUnionTypesInAssemblyContaining<'t> =
    typeof<'t>.Assembly |> addUnionTypesInAssembly

let addUnion<'t> = addUnionType typeof<'t>

let addDefaultOptionTypes () =
    addOption<byte>
    addOption<int16>
    addOption<int32>
    addOption<int64>
    addOption<bigint>

    addOption<sbyte  >
    addOption<uint16>
    addOption<uint32>
    addOption<uint64>

    addOption<Half   >
    addOption<single>
    addOption<double>
    addOption<decimal>

    addOption<bool    >
    addOption<char>
    addOption<string>
    addOption<Guid>
    addOption<Uri>
    addOption<byte[]>

    addOption<TimeSpan>
    addOption<TimeOnly>
    addOption<DateOnly>
    addOption<DateTimeOffset>
    addOption<DateTime>

    addOption<System.Numerics.Vector2>
    addOption<System.Numerics.Vector3>
    addOption<System.Numerics.Vector4>
    addOption<System.Drawing.Point>
    addOption<System.Drawing.PointF>
    addOption<System.Drawing.Color>
