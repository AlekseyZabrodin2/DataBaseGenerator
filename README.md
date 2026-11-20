# DataBase Generator

Десктопное приложение **Windows (WPF)** для подготовки **тестовых и демо-данных**: MySQL (регистрация и WorkList), **LiteDB** (пациенты, исследования, planned studies), **DICOM** (генерация файлов, импорт, PACS, просмотр тегов).

## Возможности

### DataBases

| Модуль | Назначение |
|--------|------------|
| **MySql Generator** | Пакетная и ручная генерация пациентов и **WorkList** (модальность, AE Title); настройки MySQL в **Settings**. |
| **LiteDB — Patients** | Пациенты в файле `.db`, параметры генерации (языки имён, граничные данные, возраст). |
| **LiteDB — Studies** | Выполненные исследования в том же LiteDB. |
| **LiteDB — Planned studies** | Запланированные съёмки: только БД или с заглушками `.dcm` в `C:\DicomTempFolder`. |
| **Просмотр LiteDB** | Просмотр коллекций без генерации (Update / Autoupdate). |

### Dicom Generator

| Модуль | Назначение |
|--------|------------|
| **DICOM generate** | Синтетические `.dcm` из папки **TestData** в выбранный каталог. |
| **DICOM parser** | Импорт в LiteDB из папок и **PACS** (C-FIND, метаданные); **Scan sources** → **Save to DB**. |
| **PACS mover** | Отправка файлов на PACS (**C-ECHO**, **C-STORE**), превью по исследованиям. |
| **DICOM reader** | Просмотр DICOM-тегов без записи в БД. |

## Состав репозитория

- `DataBaseGenerator` — UI (WPF, MahApps.Metro, FellowOakDicom).
- `DataBaseGenerator.Core` — генераторы, правила, LiteDB, planned studies, DICOM stubs.
- `DataBaseGenerator.Test` — модульные тесты.

## Сборка и запуск

Требуется **.NET 9** (Windows).

```powershell
dotnet build DataBaseGenerator.sln -c Release
dotnet run --project DataBaseGenerator\DataBaseGenerator.UI.Wpf.csproj -c Release
```

Готовый exe после сборки:  
`DataBaseGenerator\bin\Release\net9.0-windows10.0.17763.0\DataBaseGenerator.UI.Wpf.exe`


### Контрибьюция

Если у вас есть идеи или улучшения, создавайте issue или отправляйте pull request или свяжитесь с нами через электронную почту: alekseyzabrodin2.0@gmail.com