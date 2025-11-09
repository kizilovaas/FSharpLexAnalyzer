namespace FSharpLexAnalyzer.UI

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Shapes
open System.Windows.Media

module Chart =

    let colors = [| 
        Brushes.Red; Brushes.Blue; Brushes.Green; Brushes.Brown; 
        Brushes.Purple; Brushes.Orange; Brushes.Teal; Brushes.Magenta 
    |]

    let drawMultipleCharts (canvas: Canvas) (profiles: (string * (int * float) list * Brush) list) (currentData: (int * float) list) =
        canvas.Children.Clear()
        if profiles.IsEmpty && currentData.IsEmpty then () else

        let width = if canvas.ActualWidth > 0.0 then canvas.ActualWidth else 400.0
        let height = if canvas.ActualHeight > 0.0 then canvas.ActualHeight else 250.0
        let margin = 30.0
        let plotWidth = width - 2.0 * margin
        let plotHeight = height - 2.0 * margin

        // Собираем все данные для масштабирования
        let allDataPoints = 
            [ for _, data, _ in profiles do yield! data ]
            @ currentData

        if allDataPoints.IsEmpty then () else

        let xValues = allDataPoints |> List.map fst |> List.map float
        let yValues = allDataPoints |> List.map snd

        let minX = xValues |> List.min
        let maxX = xValues |> List.max
        let maxY = if yValues.IsEmpty then 1.0 else yValues |> List.max

        let scaleX x = margin + (x - minX) / (maxX - minX) * plotWidth
        let scaleY y = height - margin - (y / maxY) * plotHeight

        // Рисуем профили из JSON
        for profileName, data, color in profiles do
            let polyline = Polyline()
            polyline.Stroke <- color
            polyline.StrokeThickness <- 1.5
            polyline.StrokeDashArray <- DoubleCollection([| 4.0; 2.0 |]) // Пунктир для профилей

            data |> List.iter (fun (x, y) ->
                polyline.Points.Add(Point(scaleX (float x), scaleY y))
            )
            canvas.Children.Add(polyline) |> ignore

            // Добавляем легенду
            if not data.IsEmpty then
                let legend = TextBlock(Text = profileName, FontSize = 9.0, Foreground = color)
                Canvas.SetLeft(legend, width - 100.0)
                Canvas.SetTop(legend, 10.0 + float (profiles |> List.findIndex (fun (n, _, _) -> n = profileName)) * 15.0)
                canvas.Children.Add(legend) |> ignore

        // Рисуем текущие данные поверх
        if not currentData.IsEmpty then
            let currentPolyline = Polyline()
            currentPolyline.Stroke <- Brushes.Black
            currentPolyline.StrokeThickness <- 2.5

            currentData |> List.iter (fun (x, y) ->
                currentPolyline.Points.Add(Point(scaleX (float x), scaleY y))
            )
            canvas.Children.Add(currentPolyline) |> ignore

            // Точки для текущих данных
            for (x, y) in currentData do
                let ellipse = Ellipse()
                ellipse.Width <- 6.0
                ellipse.Height <- 6.0
                ellipse.Fill <- Brushes.Black
                Canvas.SetLeft(ellipse, scaleX (float x) - 3.0)
                Canvas.SetTop(ellipse, scaleY y - 3.0)
                canvas.Children.Add(ellipse) |> ignore

        // Оси и подписи
        let xAxis = Line()
        xAxis.X1 <- margin
        xAxis.Y1 <- height - margin
        xAxis.X2 <- width - margin
        xAxis.Y2 <- height - margin
        xAxis.Stroke <- Brushes.Black
        canvas.Children.Add(xAxis) |> ignore

        let yAxis = Line()
        yAxis.X1 <- margin
        yAxis.Y1 <- margin
        yAxis.X2 <- margin
        yAxis.Y2 <- height - margin
        yAxis.Stroke <- Brushes.Black
        canvas.Children.Add(yAxis) |> ignore

        // Подписи по оси X
        let xLabels = 
            if not currentData.IsEmpty then currentData 
            else if not profiles.IsEmpty then profiles.Head |> fun (_, data, _) -> data 
            else []
            
        for (x, _) in xLabels do
            let label = TextBlock(Text = string x, FontSize = 10.0)
            Canvas.SetLeft(label, scaleX (float x) - 5.0)
            Canvas.SetTop(label, height - margin + 5.0)
            canvas.Children.Add(label) |> ignore

        // Ось Y
        let yStep = maxY / 4.0
        for i in 0..4 do
            let value = float i * yStep
            let label = TextBlock(Text = sprintf "%.1f%%" value, FontSize = 9.0)
            Canvas.SetLeft(label, 2.0)
            Canvas.SetTop(label, scaleY value - 7.0)
            canvas.Children.Add(label) |> ignore