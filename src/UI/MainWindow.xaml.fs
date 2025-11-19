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
        let lexData = letterFrequencyStats text
        drawChartWithProfiles lexicalDiversityChart "Лекс. разнообр." lexData

        // 3️⃣ Частота местоимений
        let pronData = sentenceLengthStats text
        drawChartWithProfiles pronounChart "Местоимения" pronData

        // 4️⃣ Уникальность слов
        let uniqData = specialCharsStats text
        drawChartWithProfiles uniquenessChart "Уникальность" uniqData

        // ----------------------
        // ФОРМИРУЕМ JSON ДАННЫЕ В НУЖНОМ ФОРМАТЕ
        // ----------------------
        
        // Функция для создания массива в правильном формате
        let createArray (data: (int * float) list) =
            let array = Array.zeroCreate 11
            array.[0] <- 100.0 // первое значение всегда 100
            for (index, value) in data do
                if index < 11 && index > 0 then // index 0 уже занят 100.0
                    array.[index] <- value
            array

        let wlArray = createArray wlData
        let lexArray = createArray lexData
        let pronArray = createArray pronData
        let uniqArray = createArray uniqData
        
        // Поле "5" - создаем массив из 11 элементов
        let avgSentLen = avgSentenceLength text
        let lexDiv = lexicalDiversity text
        let uniqCoeff = uniquenessCoefficient text
        let wordCount = countWords text
        let charCount = countSymbol text
        
        let metricsArray = [|
            100.0;
            avgSentLen;
            lexDiv * 100.0;
            uniqCoeff;
            float wordCount / 10.0;
            float charCount / 100.0;
            0.0; 0.0; 0.0; 0.0; 0.0
        |]
        
        // Функция для правильного форматирования массива
        let formatArray (arr: float[]) =
            let elements = 
                arr 
                |> Array.map (fun x -> 
                    if x = float (int x) then 
                        sprintf "%.0f" x  // Целое число без .0
                    else 
                        sprintf "%.2f" x) // Дробное число с 2 знаками
                |> String.concat ", "
            "[ " + elements + " ]"
        
        // Формируем JSON строку в точном формате
        let jsonText = 
            "{\n" +
            "  \"id\": 0,\n" +
            "  \"name\": \"Название вашего текста\",\n" +
            "  \"color\": \"Blue\",\n" +
            "  \"1\": " + (formatArray wlArray) + ",\n" +
            "  \"2\": " + (formatArray lexArray) + ",\n" +
            "  \"3\": " + (formatArray pronArray) + ",\n" +
            "  \"4\": " + (formatArray uniqArray) + ",\n" +
            "  \"5\": " + (formatArray metricsArray) + "\n" +
            "}"
        
        // ЗАМЕНЯЕМ ТЕКСТ В ТЕКСТОВОМ ПОЛЕ НА JSON ДАННЫЕ
        //textBox.Text <- jsonText
        
        // Показываем статистику в MessageBox
        let statsMessage = 
            sprintf "Статистика текста:\n\n" +
            sprintf "Символов: %d\n" charCount +
            sprintf "Слов: %d\n" wordCount +
            sprintf "Средняя длина предложения: %.2f\n" avgSentLen +
            sprintf "Лексическое разнообразие: %.2f%%\n" (lexDiv * 100.0) +
            sprintf "Коэффициент уникальности: %.2f\n\n" uniqCoeff +
            sprintf "JSON данные отображены в текстовом поле"
        
        MessageBox.Show(statsMessage, "Анализ завершен") |> ignore
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