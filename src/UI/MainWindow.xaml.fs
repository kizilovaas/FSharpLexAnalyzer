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

    // ----------------------
    // ФУНКЦИИ АНАЛИЗА СХОДСТВА (ИСПРАВЛЕНО)
    // ----------------------

    /// Вычисляет Среднюю Абсолютную Ошибку (MAE) между двумя массивами профилей.
    /// profileA (анализируемый) имеет смещение на 1 (индекс 0 - заглушка).
    let calculateMAE (profileA: float[]) (profileB: float[]) : float =
        // profileA имеет длину 11 (индексы 0-10). profileB имеет длину 10 (индексы 0-9).
        // Сравниваем profileA[i+1] с profileB[i] для i от 0 до 9.
        
        let comparisonLength = Math.Min(profileA.Length - 1, profileB.Length)
        
        // Создаем массив разностей, сравнивая profileA[i+1] с profileB[i]
        let differences = 
            Array.init comparisonLength (fun i -> 
                let analyzedValue = profileA.[i + 1] // Начинаем с индекса 1 (пропускаем profileA.[0])
                let jsonValue = profileB.[i]       // Начинаем с индекса 0
                Math.Abs(analyzedValue - jsonValue)
            )
            
        if comparisonLength = 0 then 0.0 else differences |> Array.average


    /// Вычисляет общий процент сходства по 4 профилям
    let calculateSimilarity (profile: TextProfile) (analyzed: TextProfile) : float =
        
        // 1. Сравниваем 4 основные метрики (1, 2, 3, 4)
        let mae1 = calculateMAE analyzed.``1`` profile.``1``
        let mae2 = calculateMAE analyzed.``2`` profile.``2``
        let mae3 = calculateMAE analyzed.``3`` profile.``3``
        let mae4 = calculateMAE analyzed.``4`` profile.``4``
        
        // 2. Вычисляем общее среднее абсолютное отклонение
        let totalMAE = (mae1 + mae2 + mae3 + mae4) / 4.0
        
        // 3. Переводим ошибку в процент сходства: 100% - Avg.MAE
        let similarity = 100.0 - totalMAE
        
        Math.Max(0.0, similarity)

    // ----------------------
    // ОСНОВНЫЕ ФУНКЦИИ UI
    // ----------------------

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
            // Устанавливаем лимит для оси X, чтобы избежать 'улетания' графика
            let MAX_X_CHART_LIMIT = 10 
            
            // --- 1. РАСЧЕТ И ОГРАНИЧЕНИЕ ДАННЫХ ДЛЯ ГРАФИКОВ ---
            
            // 1️⃣ Длины слов
            let wlData = 
                wordLengthStats text
                |> List.filter (fun (metric, _) -> metric <= MAX_X_CHART_LIMIT)
            drawChartWithProfiles wordLengthChart "Длины слов" wlData

            // 2️⃣ Частота букв
            let lexData = 
                letterFrequencyStats text
                |> List.filter (fun (metric, _) -> metric <= MAX_X_CHART_LIMIT)
            drawChartWithProfiles lexicalDiversityChart "Лекс. разнообр." lexData

            // 3️⃣ Длины предложений
            let pronData = 
                sentenceLengthStats text
                |> List.filter (fun (metric, _) -> metric <= MAX_X_CHART_LIMIT)
            drawChartWithProfiles pronounChart "Местоимения" pronData

            // 4️⃣ Спец. символы
            let uniqData = 
                specialCharsStats text
                |> List.filter (fun (metric, _) -> metric <= MAX_X_CHART_LIMIT)
            drawChartWithProfiles uniquenessChart "Уникальность" uniqData

            // --- 2. ФОРМИРОВАНИЕ JSON ДАННЫХ В НУЖНОМ ФОРМАТЕ ---

            // Функция для создания массива в правильном формате (исправлено ранее)
            let createArray (data: (int * float) list) =
                let array = Array.zeroCreate 11
                array.[0] <- 100.0 
                
                for (metric, value) in data do 
                    if metric >= 1 && metric <= 10 then 
                        array.[metric] <- value
                
                array

            let wlArray = createArray wlData
            let lexArray = createArray lexData
            let pronArray = createArray pronData
            let uniqArray = createArray uniqData
            
            // Поле "5" - массив общих метрик 
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
                0.0; 0.0; 0.0; 0.0;
                0.0
            |]
            
            // --- 3. ВЫЧИСЛЕНИЕ СХОДСТВА С ПРОФИЛЯМИ ---
            
            // Создаем временный профиль для сравнения
            let analyzedProfile = 
                { id = -1; name = "Current Text"; color = "Black";
                  ``1`` = wlArray; ``2`` = lexArray; ``3`` = pronArray; ``4`` = uniqArray; ``5`` = metricsArray }

            // Вычисляем процент сходства для всех загруженных профилей
            let similarities = 
                profiles 
                |> List.map (fun p -> 
                    let similarity = calculateSimilarity p analyzedProfile
                    (p.name, similarity)
                )
                |> List.sortByDescending snd // Сортируем по убыванию процента

            // Формируем результирующее сообщение
            let topMatch = 
                match similarities with
                | (name, percent)::_ -> sprintf "%s (Сходство: %.2f%%)" name percent
                | _ -> "Не удалось определить тип"

            let resultMessage = 
                let header = "✅ Результаты анализа текста:\n\n"
                let topMatchText = sprintf "Наиболее похожий тип:\n\t%s\n\n" topMatch
                let allMatchesText = 
                    similarities 
                    |> List.map (fun (name, percent) -> sprintf "• %s: %.2f%%" name percent)
                    |> String.concat "\n"
                
                header + topMatchText + "📊 Сходство по профилям (по убыванию):\n" + allMatchesText

            // Выводим результат итогового анализа
            MessageBox.Show(resultMessage, "Анализ завершен") |> ignore
    )

    // ----------------------
    // Отправка текста (оставлена без изменений)
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