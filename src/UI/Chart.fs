namespace FSharpLexAnalyzer.UI

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Shapes
open System.Windows.Media

module Chart =

    /// Преобразует текст в данные: длина слова → процент встречаемости
    let wordStats (text: string) =
        text.Split([|' '; '\n'; '\t'; ','; '.'; '!'; '?'; ':'; ';'; '"'|],
                   StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun w -> w.Length)
        |> Array.groupBy id
        |> Array.map (fun (len, arr) -> len, float arr.Length)
        |> Array.sortBy fst
        |> fun arr ->
            let total = arr |> Array.sumBy snd
            arr |> Array.map (fun (len, count) -> len, count / total * 100.0)
            |> Array.toList

    /// Рисует столбчатую диаграмму на Canvas
    let drawWordLengthChart (canvas: Canvas) (data: (int * float) list) =
        canvas.Children.Clear()
        if data.IsEmpty then () else

        let width = if canvas.ActualWidth > 0.0 then canvas.ActualWidth else 400.0
        let height = if canvas.ActualHeight > 0.0 then canvas.ActualHeight else 250.0
        let barWidth = width / float data.Length
        let maxPercent = data |> List.map snd |> List.max

        for i, (len, pct) in List.indexed data do
            let barHeight = pct / maxPercent * (height - 20.0)

            // столбец
            let rect = Rectangle()
            rect.Width <- barWidth - 4.0
            rect.Height <- barHeight
            rect.Fill <- Brushes.SteelBlue
            Canvas.SetLeft(rect, float i * barWidth + 2.0)
            Canvas.SetTop(rect, height - barHeight)
            canvas.Children.Add(rect) |> ignore

            // подпись длины слова под столбцом
            let label = TextBlock(Text = string len, FontSize = 10.0)
            Canvas.SetLeft(label, float i * barWidth + barWidth / 2.0 - 5.0)
            Canvas.SetTop(label, height - 15.0)
            canvas.Children.Add(label) |> ignore
