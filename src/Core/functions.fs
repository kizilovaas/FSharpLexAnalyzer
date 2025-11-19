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

/// Распределение частоты букв для графика
let letterFrequencyStats (text: string) : (int * float) list =
    let letters = 
        text.ToLower().ToCharArray()
        |> Array.filter Char.IsLetter
    
    let totalLetters = letters.Length
    
    if totalLetters = 0 then []
    else
        letters
        |> Array.groupBy id
        |> Array.mapi (fun i (letter, occurrences) -> i, float (Array.length occurrences) / float totalLetters * 100.0)
        |> Array.sortBy fst
        |> Array.toList

/// Распределение количества слов в предложениях для графика
let sentenceLengthStats (text: string) : (int * float) list =
    let sentences = 
        Regex.Split(text, @"[.!?]+") 
        |> Array.filter (fun s -> s.Trim() <> "")
        |> Array.map (fun s -> countWords s)
    
    let totalSentences = sentences.Length
    
    if totalSentences = 0 then []
    else
        sentences
        |> Array.groupBy id
        |> Array.map (fun (wordCount, sents) -> wordCount, float (Array.length sents) / float totalSentences * 100.0)
        |> Array.sortBy fst
        |> Array.toList

/// Распределение специальных символов для графика
let specialCharsStats (text: string) : (int * float) list =
    let specialChars = 
        text.ToCharArray()
        |> Array.filter (fun c -> not (Char.IsLetterOrDigit(c)) && not (Char.IsWhiteSpace(c)))
    
    let totalSpecialChars = specialChars.Length
    
    if totalSpecialChars = 0 then []
    else
        specialChars
        |> Array.groupBy id
        |> Array.sortBy (fun (char, _) -> char)
        |> Array.mapi (fun i (char, occurrences) -> 
            i,  // X = порядковый номер символа
            float (Array.length occurrences) / float totalSpecialChars * 100.0)  // Y = процент
        |> Array.toList
