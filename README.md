# F#LexAnalyzer
Проект по созданию лексического анализатора текста на языке F#.

## Запуск F# с WPF
1.  IDE Visual Studio
2.  Если нет компонета F# - добавляем
3.  Создаем проект на F#
4.  Выбираем консольное приложение для Windows (.NET Framework)
5.  Версия NET 8.0
6.  В свойствах проекта настраиваем:
    - Тип вывода - Приложение Windows
    - Целевая ОС - Windows
    - Windows Presentation Foundation (WPF)
7.  В проекте добавляем файлы:
    - MainWindow.xaml - верстка GUI
    - MainWindow.xaml.fs - функции GUI
8.  В свойствах файла MainWindow.xaml настраиваем
    - Действие при сборке - Ресурс
9.  В файле FlexAnalyzer.fsproj добавляем
    - < Resource Include="data\MainWindow.xaml" />
    - < Compile Include="data\MainWindow.xaml.fs" />
