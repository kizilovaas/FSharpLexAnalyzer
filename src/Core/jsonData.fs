module jsonData

open System.IO
open System.Text.Json

type TextProfile = {
    id: int
    name: string
    color: string
    ``1``: float[]
    ``2``: float[]
    ``3``: float[]
    ``4``: float[]
    ``5``: float[]
}

let getJsonFilePath () =
    Path.Combine(__SOURCE_DIRECTORY__, "textProfiles.json")

let loadTextProfiles () : TextProfile list =
    let path = getJsonFilePath()
    if File.Exists path then
        let text = File.ReadAllText(path)
        try
            JsonSerializer.Deserialize<TextProfile[]>(text) |> Array.toList
        with
        | _ -> []
    else []
