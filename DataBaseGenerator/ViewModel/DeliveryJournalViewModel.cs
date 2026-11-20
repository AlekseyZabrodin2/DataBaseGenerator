using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.DeliveryJournal;
using DataBaseGenerator.UI.Wpf.ViewModel.Options;
using Microsoft.Win32;

namespace DataBaseGenerator.UI.Wpf.ViewModel;

public partial class DeliveryJournalViewModel : ObservableObject
{
    private readonly GenerationOptionsStore _optionsStore;

    [ObservableProperty]
    public partial ObservableCollection<DeliveryJournalRow> Rows { get; set; } = new();
    public ObservableCollection<string> StatusFilters { get; } = new()
    {
        "All", "Delivered", "Failed", "Pending", "Sending", "NotRequired"
    };

    public ObservableCollection<string> ImportStatusOptions { get; } = new()
    {
        "Delivered", "Failed", "Pending", "Sending", "NotRequired"
    };

    [ObservableProperty]
    public partial string DatabasePath { get; set; }

    [ObservableProperty]
    public partial string DatabasePathShort { get; set; }

    [ObservableProperty]
    public partial string SelectedStatusFilter { get; set; } = "All";

    [ObservableProperty]
    public partial string SelectedImportStatus { get; set; } = "Delivered";

    [ObservableProperty]
    public partial string PatientIdFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DeliveryJournalRow? SelectedRow { get; set; }

    [ObservableProperty]
    public partial string DetailText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UpdateText { get; set; } = "Выберите delivery.db и нажмите Update data.";

    [ObservableProperty]
    public partial bool IsReadOnlyMode { get; set; } = true;

    [ObservableProperty]
    public partial int ApplyStatusCount { get; set; }

    [ObservableProperty]
    public partial int ImportFromStudiesCount { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string BusyMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int CurrentProgress { get; set; }

    [ObservableProperty]
    public partial bool PercentShow { get; set; }

    [ObservableProperty]
    public partial bool OptimisIsVisibility { get; set; }

    partial void OnSelectedRowChanged(DeliveryJournalRow? value) => _ = LoadDetailAsync();

    partial void OnSelectedStatusFilterChanged(string value) => ApplyViewFilter();

    partial void OnPatientIdFilterChanged(string value) => ApplyViewFilter();

    partial void OnDatabasePathChanged(string value)
    {
        UpdatePathDisplay();
        if (!string.IsNullOrWhiteSpace(value))
            _optionsStore.SetDeliveryDatabasePath(value);
    }

    public DeliveryJournalViewModel(GenerationOptionsStore optionsStore)
    {
        _optionsStore = optionsStore;
        DatabasePath = optionsStore.DeliveryDatabasePath;
        UpdatePathDisplay();
    }

    [RelayCommand]
    private void BrowseDatabase()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "LiteDB files (*.db)|*.db|All files (*.*)|*.*",
            Title = "delivery.db",
            FileName = string.IsNullOrWhiteSpace(DatabasePath) ? "delivery.db" : Path.GetFileName(DatabasePath)
        };

        if (dialog.ShowDialog() != true)
            return;

        DatabasePath = dialog.FileName;
        _ = RefreshDataBaseAsync();
    }

    [RelayCommand]
    private async Task RefreshDataBaseAsync()
    {
        if (string.IsNullOrWhiteSpace(DatabasePath) || !File.Exists(DatabasePath))
        {
            UpdateText = "Файл delivery.db не найден.";
            Rows.Clear();
            DetailText = string.Empty;
            return;
        }

        try
        {
            StartBusy("Загрузка журнала доставки…", percentShow: false);

            var rows = await Task.Run(() => DeliveryJournalService.LoadRows(DatabasePath, statusFilter: null));
            var keepUid = SelectedRow?.StudyInstanceUid;

            Rows = new ObservableCollection<DeliveryJournalRow>(rows);
            ApplyViewFilter(updateStatus: false);
            UpdateJournalStatistics();
            SelectedRow = keepUid == null
                ? Rows.FirstOrDefault()
                : Rows.FirstOrDefault(r => r.StudyInstanceUid == keepUid) ?? Rows.FirstOrDefault();
        }
        catch (Exception ex)
        {
            UpdateText = $"Ошибка чтения: {ex.Message}";
        }
        finally
        {
            StopBusy();
        }
    }

    [RelayCommand]
    private void ChangeDatadaseMode(object? readOnlyFromToggle)
    {
        var readOnly = readOnlyFromToggle switch
        {
            bool b => b,
            _ => IsReadOnlyMode
        };

        IsReadOnlyMode = readOnly;
        UpdateText = $"Режим переключен: {(readOnly ? "Только чтение" : "Чтение/Запись")}.";
    }

    [RelayCommand]
    private async Task ImportFromStudiesAsync()
    {
        if (IsReadOnlyMode)
        {
            UpdateText = "Выключите режим «Только чтение» (переключатель Mode).";
            return;
        }

        var sharedLiteDb = ResolveSharedLiteDbPath();
        if (sharedLiteDb is null)
        {
            UpdateText = "Нет LiteDB со studies: задайте Patients DB в LiteDB → Patients или в Settings.";
            return;
        }

        if (string.IsNullOrWhiteSpace(DatabasePath) || !File.Exists(DatabasePath))
        {
            UpdateText = "Файл delivery.db не найден.";
            return;
        }

        var count = ImportFromStudiesCount <= 0 ? 100 : ImportFromStudiesCount;
        var importStatus = ParseStatus(SelectedImportStatus)
            ?? PacsDeliveryStatus.Delivered;
        var dicomRoot = string.IsNullOrWhiteSpace(_optionsStore.DicomSaveFolderPath)
            ? DeliveryJournalStudiesImport.DefaultDicomRoot
            : _optionsStore.DicomSaveFolderPath;

        var stopwatch = Stopwatch.StartNew();
        var progress = new Progress<int>(percent => CurrentProgress = percent);
        string? operationSummary = null;
        try
        {
            StartBusy("Импорт из studies…");
            UpdateText =
                $"Импорт до {count} ({importStatus})… {Path.GetFileName(sharedLiteDb)}, DICOM: {dicomRoot}.";
            var result = await Task.Run(() =>
                DeliveryJournalService.AddFromStudiesDatabase(
                    sharedLiteDb,
                    DatabasePath,
                    count,
                    importStatus,
                    dicomRoot,
                    progress));

            operationSummary =
                $"Добавлено: {result.Added}. Уже были: {result.SkippedExisting}. Без SnapshotPatientId: {result.SkippedNoData}.";
            await RefreshDataBaseAsync();
        }
        catch (Exception ex)
        {
            UpdateText = $"Ошибка импорта: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            StopBusy();
            if (operationSummary != null)
                UpdateText = $"{operationSummary} Время выполнения: {FormatTimeSpan(stopwatch.Elapsed)}";
        }
    }

    [RelayCommand]
    private async Task ClearAllRecordsAsync()
    {
        if (IsReadOnlyMode)
        {
            UpdateText = "Выключите режим «Только чтение» (переключатель Mode).";
            return;
        }

        if (string.IsNullOrWhiteSpace(DatabasePath) || !File.Exists(DatabasePath))
        {
            UpdateText = "Файл delivery.db не найден.";
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        string? operationSummary = null;
        try
        {
            StartBusy("Очистка delivery_records…", percentShow: false);
            UpdateText = "Удаление всех записей delivery_records… Закройте клиент, если файл заблокирован.";

            var deleted = await Task.Run(() => DeliveryJournalService.ClearAllRecords(DatabasePath));

            DetailText = string.Empty;
            SelectedRow = null;
            operationSummary = deleted > 0
                ? $"Журнал очищен: удалено {deleted} записей."
                : "Журнал уже пуст (delivery_records).";
            await RefreshDataBaseAsync();
        }
        catch (Exception ex)
        {
            UpdateText = $"Ошибка очистки: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            StopBusy();
            if (operationSummary != null)
                UpdateText = $"{operationSummary} Время выполнения: {FormatTimeSpan(stopwatch.Elapsed)}";
        }
    }

    [RelayCommand]
    private async Task ApplySelectedStatusAsync()
    {
        if (IsReadOnlyMode)
        {
            UpdateText = "Выключите режим «Только чтение» (переключатель Mode).";
            return;
        }

        if (string.IsNullOrWhiteSpace(DatabasePath) || !File.Exists(DatabasePath))
        {
            UpdateText = "Файл delivery.db не найден.";
            return;
        }

        var targetStatus = ParseStatus(SelectedImportStatus)
            ?? PacsDeliveryStatus.Delivered;
        var count = ApplyStatusCount <= 0 ? 100 : ApplyStatusCount;
        var studyUids = GetFilteredStudyUidsInDisplayOrder();
        var visibleTotal = studyUids.Count;
        if (visibleTotal == 0)
        {
            UpdateText = "Нет записей в текущей выборке (Показать / ID пациента). Нажмите Update data.";
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var progress = new Progress<int>(percent => CurrentProgress = percent);
        string? operationSummary = null;
        try
        {
            StartBusy("Обновление статусов…");
            UpdateText =
                $"Смена статуса: до {count} из {visibleTotal} в выборке → {SelectedImportStatus}…";
            var uids = studyUids;
            var (updated, available) = await Task.Run(() =>
                DeliveryJournalService.ApplyStatusToRecordsByStudyUids(
                    DatabasePath,
                    targetStatus,
                    count,
                    uids,
                    progress));

            var patientHint = string.IsNullOrWhiteSpace(PatientIdFilter)
                ? string.Empty
                : $", ID «{PatientIdFilter.Trim()}»";
            operationSummary = $"Статус {SelectedImportStatus}: обновлено {updated} из {available}.";

            await RefreshDataBaseAsync();
        }
        catch (Exception ex)
        {
            UpdateText = $"Ошибка записи: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            StopBusy();
            if (operationSummary != null)
                UpdateText = $"{operationSummary} Время выполнения: {FormatTimeSpan(stopwatch.Elapsed)}";
        }
    }

    private async Task LoadDetailAsync()
    {
        if (SelectedRow == null || string.IsNullOrWhiteSpace(DatabasePath))
        {
            DetailText = string.Empty;
            return;
        }

        try
        {
            var record = await Task.Run(() =>
                DeliveryJournalService.LoadRecord(DatabasePath, SelectedRow.StudyInstanceUid));
            if (record == null)
            {
                DetailText = string.Empty;
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Study: {record.StudyInstanceUid}");
            sb.AppendLine($"Patient: {record.PatientId}");
            sb.AppendLine($"Status: {record.Status}, Failure: {record.FailureKind}");
            sb.AppendLine($"Error: {record.ErrorMessage}");
            sb.AppendLine($"Targets: {string.Join(", ", record.TargetKeys ?? [])}");
            foreach (var file in record.Files)
            {
                sb.AppendLine();
                sb.AppendLine($"  File: {file.FilePath}");
                sb.AppendLine($"  SOP: {file.SopInstanceUid}");
                foreach (var td in file.TargetDeliveries)
                    sb.AppendLine($"    → {td.TargetKey}: {td.Status} ({td.FailureKind}) {td.ErrorMessage}");
            }

            DetailText = sb.ToString();
        }
        catch (Exception ex)
        {
            DetailText = ex.Message;
        }
    }

    /// <summary>Study UIDs in the same order as the filtered grid (Показать + ID пациента).</summary>
    private IReadOnlyList<string> GetFilteredStudyUidsInDisplayOrder()
    {
        if (Rows.Count == 0)
            return Array.Empty<string>();

        var view = CollectionViewSource.GetDefaultView(Rows);
        if (view is CollectionView { Filter: not null })
            return view.Cast<DeliveryJournalRow>().Select(r => r.StudyInstanceUid).ToList();

        if (view.Filter is not null)
        {
            return Rows
                .Where(r => view.Filter(r))
                .Select(r => r.StudyInstanceUid)
                .ToList();
        }

        return Rows.Select(r => r.StudyInstanceUid).ToList();
    }

    private void ApplyViewFilter(bool updateStatus = true)
    {
        var view = CollectionViewSource.GetDefaultView(Rows);
        var patientFilter = PatientIdFilter?.Trim();
        var statusFilter = ParseFilter(SelectedStatusFilter);

        if (string.IsNullOrEmpty(patientFilter) && statusFilter is null)
            view.Filter = null;
        else
            view.Filter = item =>
            {
                var row = (DeliveryJournalRow)item;
                if (!string.IsNullOrEmpty(patientFilter)
                    && row.PatientId?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) != true)
                    return false;

                return statusFilter is null || row.Status == statusFilter.Value;
            };

        if (updateStatus)
            UpdateJournalStatistics();
    }

    private void UpdateJournalStatistics()
    {
        var total = Rows.Count;
        var visible = VisibleRowCount(Rows);
        var mode = IsReadOnlyMode ? "только чтение" : "чтение/запись";
        var viewFiltered = visible != total
            || ParseFilter(SelectedStatusFilter) is not null
            || !string.IsNullOrWhiteSpace(PatientIdFilter);

        if (!viewFiltered)
            UpdateText = $"Записей: {total}. Режим: {mode}.";
        else
            UpdateText = $"Показано: {visible} из {total} (статус: {SelectedStatusFilter}). Режим: {mode}.";
    }

    private static int VisibleRowCount(object source)
    {
        if (source == null)
            return 0;

        return CollectionViewSource.GetDefaultView(source) is CollectionView view
            ? view.Count
            : 0;
    }

    private void UpdatePathDisplay()
    {
        if (string.IsNullOrWhiteSpace(DatabasePath))
        {
            DatabasePathShort = string.Empty;
            return;
        }

        DatabasePathShort = DatabasePath.Length > 70
            ? "…" + DatabasePath[^Math.Min(67, DatabasePath.Length)..]
            : DatabasePath;
    }

    private string? ResolveSharedLiteDbPath()
    {
        var path = _optionsStore.PatientsDatabasePath;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            return path;

        path = App.SharedStorage?.DatabasePath;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            return path;

        return null;
    }

    private void StartBusy(string message, bool percentShow = true)
    {
        CurrentProgress = 0;
        PercentShow = percentShow;
        BusyMessage = message;
        IsBusy = true;
    }

    private void StopBusy()
    {
        IsBusy = false;
        BusyMessage = string.Empty;
        CurrentProgress = 0;
        PercentShow = false;
    }

    private static string FormatTimeSpan(TimeSpan time)
    {
        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours} ч {time.Minutes} мин {time.Seconds} сек";
        if (time.TotalMinutes >= 1)
            return $"{time.Minutes} мин {time.Seconds} сек";
        return $"{time.TotalSeconds:F1} сек";
    }

    private static PacsDeliveryStatus? ParseFilter(string filter) => ParseStatus(filter);

    private static PacsDeliveryStatus? ParseStatus(string? filter) => filter switch
    {
        "Delivered" => PacsDeliveryStatus.Delivered,
        "Failed" => PacsDeliveryStatus.Failed,
        "Pending" => PacsDeliveryStatus.Pending,
        "Sending" => PacsDeliveryStatus.Sending,
        "NotRequired" => PacsDeliveryStatus.NotRequired,
        _ => null
    };
}
