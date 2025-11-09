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
    let btnSend = window.FindName("btnSend") :?> Button
    let cmbTextType = window.FindName("cmbTextType") :?> ComboBox
    let textBox = window.FindName("textBox") :?> TextBox

    let wordLengthChart = window.FindName("wordLengthChart") :?> Canvas
    let lexicalDiversityChart = window.FindName("lexicalDiversityChart") :?> Canvas
    let pronounChart = window.FindName("pronounChart") :?> Canvas
    let uniquenessChart = window.FindName("uniquenessChart") :?> Canvas

    // Загружаем профили при запуске
    let profiles = loadTextProfiles()

    // Заполняем выпадающий список
    let fillComboBox () =
        cmbTextType.Items.Clear()
        for profile in profiles do
            ComboBoxItem(Content = profile.name) |> cmbTextType.Items.Add |> ignore

    let drawChartWithProfiles (canvas: Canvas) (profileType: string) (currentData: (int * float) list) =
        let profileChartData = 
            profiles 
            |> List.collect profileToChartData
            |> List.filter (fun (name: string, _, _) -> name.Contains(profileType))
            |> List.map (fun (_, data, brush) -> ("", data, brush))
        
        drawMultipleCharts canvas profileChartData currentData

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
        cmbTextType.SelectedIndex <- -1
        // Очищаем только текущие данные, но оставляем профили
        drawChartWithProfiles wordLengthChart "Длины слов" []
        drawChartWithProfiles lexicalDiversityChart "Лекс. разнообр." []
        drawChartWithProfiles pronounChart "Местоимения" []
        drawChartWithProfiles uniquenessChart "Уникальность" []
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
            drawChartWithProfiles wordLengthChart "Длины слов" wlData

            // 2️⃣ Лексическое разнообразие
            let lexData = lexicalDiversityStats text
            drawChartWithProfiles lexicalDiversityChart "Лекс. разнообр." lexData

            // 3️⃣ Частота местоимений
            let pronData = pronounStats text
            drawChartWithProfiles pronounChart "Местоимения" pronData

            // 4️⃣ Уникальность слов
            let uniqData = uniquenessStats text
            drawChartWithProfiles uniquenessChart "Уникальность" uniqData
    )

    // ----------------------
    // Отправка текста (заглушка)
    // ----------------------
    btnSend.Click.Add(fun _ ->
        let text = textBox.Text.Trim()
        let selectedType = 
            if cmbTextType.SelectedItem <> null then
                (cmbTextType.SelectedItem :?> ComboBoxItem).Content.ToString()
            else
                "Не выбран"
        
        if String.IsNullOrEmpty(text) then
            MessageBox.Show("Введите текст для отправки", "Информация") |> ignore
        else
            // Заглушка для будущей функциональности
            let charCount = countSymbol text
            let wordCount = countWords text
            
            let message = 
                sprintf "Текст отправлен! (заглушка)\n\n" +
                sprintf "Тип: %s\n" selectedType +
                sprintf "Статистика:\n" +
                sprintf "• Символов: %d\n" charCount +
                sprintf "• Слов: %d\n" wordCount +
                sprintf "• Уникальность: %.2f\n\n" (uniquenessCoefficient text) +
                sprintf "В будущем здесь будет отправка на сервер"
            
            MessageBox.Show(message, "Отправка текста") |> ignore
            
            // Логируем в консоль
            printfn "ОТПРАВКА: тип='%s', символы=%d, слова=%d" selectedType charCount wordCount
    )

    // Инициализируем при запуске
    window.Loaded.Add(fun _ ->
        fillComboBox()
        drawChartWithProfiles wordLengthChart "Длины слов" []
        drawChartWithProfiles lexicalDiversityChart "Лекс. разнообр." []
        drawChartWithProfiles pronounChart "Местоимения" []
        drawChartWithProfiles uniquenessChart "Уникальность" []
    )

    // ----------------------
    // Возвращаем окно
    // ----------------------
    window