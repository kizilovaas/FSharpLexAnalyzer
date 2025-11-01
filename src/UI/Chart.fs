namespace FSharpLexAnalyzer.UI

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Shapes
open System.Windows.Media

module Chart =

    let drawWordLengthChart (canvas: Canvas) (data: (int * float) list) =
        canvas.Children.Clear()
        if data.IsEmpty then () else

        let width = if canvas.ActualWidth > 0.0 then canvas.ActualWidth else 400.0
        let height = if canvas.ActualHeight > 0.0 then canvas.ActualHeight else 250.0
        let margin = 30.0
        let plotWidth = width - 2.0 * margin
        let plotHeight = height - 2.0 * margin

        let xValues = data |> List.map fst |> List.map float
        let yValues = data |> List.map snd

        let minX = xValues |> List.min
        let maxX = xValues |> List.max
        let maxY = yValues |> List.max

        let scaleX x = margin + (x - minX) / (maxX - minX) * plotWidth
        let scaleY y = height - margin - (y / maxY) * plotHeight

        let polyline = Polyline()
        polyline.Stroke <- Brushes.SteelBlue
        polyline.StrokeThickness <- 2.0

        data |> List.iter (fun (x, y) ->
            polyline.Points.Add(Point(scaleX (float x), scaleY y))
        )
        canvas.Children.Add(polyline) |> ignore

        for (x, y) in data do
            let ellipse = Ellipse()
            ellipse.Width <- 6.0
            ellipse.Height <- 6.0
            ellipse.Fill <- Brushes.SteelBlue
            Canvas.SetLeft(ellipse, scaleX (float x) - 3.0)
            Canvas.SetTop(ellipse, scaleY y - 3.0)
            canvas.Children.Add(ellipse) |> ignore

        // Подписи по оси X
        for (x, _) in data do
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
