(*
    Project:
        F#LexAnalyzer - Text Analysis Application
    
    Description:
        Приложение для идентификации авторства текста
    
    Technical Stack:
        Language: F#
        Framework: .NET 8.0
        GUI: WPF
        IDE: Visual Studio 2022
    
    Team: Анастасия Кизилова, Покидько Максим, Зубарчук Виктор, Соколов Виктор.
    University: КФУ ФТИ ПИ
    Version: 1.0.0
    Created: 2025-10-08
*)

open System
open System.Windows


[<STAThread>]
[<EntryPoint>]
let main(_) =
    let app = Application()

    app.MainWindow <- 
        Uri("src/UI/MainWindow.xaml", UriKind.Relative)
        |> Application.LoadComponent
        :?> Window
        |> MainWindow.initializeWindow

    app.MainWindow.Show()
    app.Run()