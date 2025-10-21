module jsonData

open System.Text.Json
open System.IO

// Путь к файлу с профилями текста
let getJsonFilePath () = 
    Path.Combine(__SOURCE_DIRECTORY__, "textProfiles.json")

// Проверка файла на ошибки
let checkFileAccess (filePath: string) =
    if File.Exists(filePath) then
        try 
            use testStream = File.OpenRead(filePath)
            Ok filePath
        with 
        | _ -> Error [|"Нет доступа к файлу"|]
    else
        Error [|$"Файл {filePath} не найден"|]

// Список профилей текста либо ошибка
let loadTextProfiles () =
    match getJsonFilePath () |> checkFileAccess with
    | Error error -> error
    | Ok path ->
        try
            File.ReadAllText(path)
            |> JsonSerializer.Deserialize<{| id: int; name: string |}[]>
            |> Array.map (fun x -> x.name)
        with 
        | :? JsonException -> [|"Неверный формат JSON"|] 
        | _ -> [|"Ошибка чтения файла"|]