module MainWindow

open System
open System.Windows
open System.Windows.Controls
open FSharpLexAnalyzer.UI.Chart
open functions
open jsonData

let initializeWindow (window: Window) =

    // ----------------------
    // Элементы управления
    // ----------------------
    let btnInsert = window.FindName("btnInsert") :?> Button
    let btnScan = window.FindName("btnScan") :?> Button
    let btnClear = window.FindName("btnClear") :?> Button
    let textBox = window.FindName("textBox") :?> TextBox

    let wordLengthChart = window.FindName("wordLengthChart") :?> Canvas
    let lexicalDiversityChart = window.FindName("lexicalDiversityChart") :?> Canvas
    let pronounChart = window.FindName("pronounChart") :?> Canvas
    let uniquenessChart = window.FindName("uniquenessChart") :?> Canvas

    // ----------------------
    // Вставка текста из буфера
    // ----------------------
    btnInsert.Click.Add(fun _ ->
        if Clipboard.ContainsText() then textBox.Text <- Clipboard.GetText()
        else textBox.Text <- "Буфер обмена не содержит текста"
    )

    // ----------------------
    // Очистка текста и графиков
    // ----------------------
    btnClear.Click.Add(fun _ ->
        textBox.Clear()
        wordLengthChart.Children.Clear()
        lexicalDiversityChart.Children.Clear()
        pronounChart.Children.Clear()
        uniquenessChart.Children.Clear()
    )

    // ----------------------
    // Анализ текста
    // ----------------------
    btnScan.Click.Add(fun _ ->
        let text = textBox.Text.Trim()
        if String.IsNullOrEmpty(text) then
            MessageBox.Show("Введите текст для анализа", "Информация") |> ignore
        else
            // 1️⃣ Длины слов
            let wlData = wordLengthStats text
            drawWordLengthChart wordLengthChart wlData

            // 2️⃣ Лексическое разнообразие
            let lexData = lexicalDiversityStats text
            drawWordLengthChart lexicalDiversityChart lexData

            // 3️⃣ Частота местоимений
            let pronData = pronounStats text
            drawWordLengthChart pronounChart pronData

            // 4️⃣ Уникальность слов
            let uniqData = uniquenessStats text
            drawWordLengthChart uniquenessChart uniqData
    )

    // ----------------------
    // Возвращаем окно
    // ----------------------
    window
