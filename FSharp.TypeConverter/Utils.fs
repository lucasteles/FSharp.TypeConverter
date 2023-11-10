namespace FSharp.TypeConverter

module internal Option =
    let zip a b =
        match a, b with
        | Some a, Some b -> Some(a, b)
        | _ -> None
