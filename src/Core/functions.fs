module functions

open System
open System.Text.RegularExpressions

/// Подсчет символов
let countSymbol (text: string) = text.Length

/// Подсчет слов
let countWords (text: string) =
    Regex.Matches(text, @"\b\w+\b").Count

/// Средняя длина предложения
let avgSentenceLength (text: string) =
    let sentences = Regex.Split(text, @"[.!?]+") |> Array.filter (fun s -> s.Trim() <> "")
    if sentences.Length = 0 then 0.0
    else sentences |> Array.averageBy (fun s -> float (countWords s))

/// Лексическое разнообразие (доля уникальных слов)
let lexicalDiversity (text: string) =
    let words = Regex.Matches(text.ToLower(), @"\b\w+\b") |> Seq.cast<Match> |> Seq.map (fun m -> m.Value)
    let total = Seq.length words
    let unique = Seq.distinct words |> Seq.length
    if total = 0 then 0.0 else float unique / float total

/// Коэффициент уникальности слов (среднее количество повторений слова)
let uniquenessCoefficient (text: string) =
    let words = Regex.Matches(text.ToLower(), @"\b\w+\b") |> Seq.cast<Match> |> Seq.map (fun m -> m.Value)
    let total = Seq.length words
    let counts = words |> Seq.countBy id |> Seq.map snd
    if total = 0 then 0.0 else counts |> Seq.averageBy float

/// Распределение длин слов для графика
let wordLengthStats (text: string) : (int * float) list =
    let words = Regex.Matches(text, @"\b\w+\b") |> Seq.cast<Match> |> Seq.map (fun m -> m.Value)
    let total = Seq.length words
    if total = 0 then []
    else
        words
        |> Seq.groupBy (fun w -> w.Length)
        |> Seq.map (fun (len, ws) -> len, float (Seq.length ws) / float total * 100.0)
        |> Seq.sortBy fst
        |> Seq.toList

/// Распределение лексического разнообразия для графика
let lexicalDiversityStats (text: string) : (int * float) list =
    let words =
        Regex.Matches(text.ToLower(), @"\b\w+\b") |> Seq.cast<Match> |> Seq.map (fun m -> m.Value) |> Seq.toList
    let total = List.length words
    if total = 0 then []
    else
        let counts = words |> List.groupBy id |> List.map (fun (_, ws) -> List.length ws)
        counts
        |> List.groupBy id
        |> List.map (fun (freq, ws) -> freq, float (List.length ws) / float total * 100.0)
        |> List.sortBy fst

/// Распределение частоты местоимений для графика
let pronounStats (text: string) : (int * float) list =
    let pronouns = ["я"; "ты"; "он"; "она"; "мы"; "вы"; "они"]

    let words =
        Regex.Matches(text.ToLower(), @"\b\w+\b")
        |> Seq.cast<Match>
        |> Seq.map (fun m -> m.Value)
        |> Seq.toList

    // Считаем сколько каждого местоимения
    let counts =
        pronouns
        |> List.map (fun p -> words |> List.filter ((=) p) |> List.length)

    let totalPronouns = counts |> List.sum

    if totalPronouns = 0 then []
    else
        counts
        |> List.mapi (fun i c -> i, float c / float totalPronouns * 100.0)  // X = индекс, Y = процент
        |> List.filter (fun (_, perc) -> perc > 0.0)


/// Распределение уникальности слов для графика
let uniquenessStats (text: string) : (int * float) list =
    let words =
        Regex.Matches(text.ToLower(), @"\b\w+\b") |> Seq.cast<Match> |> Seq.map (fun m -> m.Value) |> Seq.toList
    let total = List.length words
    if total = 0 then []
    else
        let counts = words |> List.groupBy id |> List.map (fun (_, ws) -> List.length ws)
        counts
        |> List.groupBy id
        |> List.map (fun (freq, ws) -> freq, float (List.length ws) / float total * 100.0)
        |> List.sortBy fst
