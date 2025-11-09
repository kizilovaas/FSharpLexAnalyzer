module jsonData

open System.IO
open System.Text.Json
open System.Windows.Media

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

let colorNameToBrush (colorName: string) =
    match colorName.ToLower() with
    | "red" -> Brushes.Red
    | "blue" -> Brushes.Blue
    | "green" -> Brushes.Green
    | "brown" -> Brushes.Brown
    | "purple" -> Brushes.Purple
    | "orange" -> Brushes.Orange
    | _ -> Brushes.Gray

// Преобразуем данные профиля в формат для графиков
let profileToChartData (profile: TextProfile) : (string * (int * float) list * Brush) list =
    let brush = colorNameToBrush profile.color
    [
        ("Длины слов - " + profile.name, 
         profile.``1`` |> Array.mapi (fun i v -> (i, v)) |> Array.toList, brush)
        ("Лекс. разнообр. - " + profile.name, 
         profile.``2`` |> Array.mapi (fun i v -> (i, v)) |> Array.toList, brush)
        ("Местоимения - " + profile.name, 
         profile.``3`` |> Array.mapi (fun i v -> (i, v)) |> Array.toList, brush)
        ("Уникальность - " + profile.name, 
         profile.``4`` |> Array.mapi (fun i v -> (i, v)) |> Array.toList, brush)
    ]