using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.GeneratorRules.WorkList;
using DataBaseGenerator.Core.OrthancWorklists;
using DataBaseGenerator.UI.Wpf.ViewModel.Options;

namespace DataBaseGenerator.UI.Wpf.ViewModel;

public partial class OrthancWorklistViewModel : ObservableObject
{
    private readonly GenerationOptionsStore _optionsStore;
    private bool _loadingConnection = true;

    public ObservableCollection<RandomModalityRule> ModalityRules { get; }

    public PatientGenerationOptions GenerationOptions { get; }

    [ObservableProperty]
    public partial ObservableCollection<OrthancWorklistEntry> Rows { get; set; } = new();

    [ObservableProperty]
    public partial string BaseUrl { get; set; }

    [ObservableProperty]
    public partial string Username { get; set; }

    [ObservableProperty]
    public partial string Password { get; set; }

    [ObservableProperty]
    public partial int? WorkListCount { get; set; } = 0;

    [ObservableProperty]
    public partial RandomModalityRule SelectModality { get; set; }

    [ObservableProperty]
    public partial string SetAeTitle { get; set; } = string.Empty;

    public IReadOnlyList<string> Gender { get; } = new[] { "Man", "Female", "Other" };

    [ObservableProperty]
    public partial string SinglePatientId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SingleLastName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SingleFirstName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SingleMiddleName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? SingleBirthDate { get; set; }

    [ObservableProperty]
    public partial string SingleSex { get; set; }

    [ObservableProperty]
    public partial RandomModalityRule SingleModality { get; set; }

    [ObservableProperty]
    public partial string SingleAddress { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SingleTelephone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SingleComments { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SingleDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PatientIdFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UpdateText { get; set; } = "Укажите адрес Orthanc и нажмите Update WorkList.";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string BusyMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int CurrentProgress { get; set; }

    [ObservableProperty]
    public partial bool PercentShow { get; set; }

    partial void OnBaseUrlChanged(string value) => SaveConnection();

    partial void OnUsernameChanged(string value) => SaveConnection();

    partial void OnPasswordChanged(string value) => SaveConnection();

    partial void OnPatientIdFilterChanged(string value) => ApplyFilter();


    public OrthancWorklistViewModel(GenerationOptionsStore optionsStore)
    {
        _optionsStore = optionsStore;
        var dx = new RandomModalityRule("DX");
        ModalityRules = new ObservableCollection<RandomModalityRule>
        {
            dx,
            new RandomModalityRule("MG")
        };
        SelectModality = dx;
        SingleModality = dx;
        GenerationOptions = optionsStore.Orthanc;
        BaseUrl = optionsStore.OrthancBaseUrl;
        Username = optionsStore.OrthancUsername;
        Password = optionsStore.OrthancPassword;
        _loadingConnection = false;
    }


    [RelayCommand]
    private async Task EchoAsync()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            UpdateText = "Укажите адрес Orthanc.";
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        string operationSummary = null;
        try
        {
            StartBusy("Проверка соединения…", percentShow: false);
            using var client = OpenClient();
            await client.PingAsync();
            operationSummary = "Соединение с Orthanc установлено.";
        }
        catch (Exception ex)
        {
            UpdateText = $"Ошибка подключения к Orthanc: {ex.Message}";
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
    private async Task AddWorkListAsync()
    {
        if (WorkListCount is not > 0)
        {
            UpdateText = "Укажите число заданий.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SetAeTitle))
        {
            UpdateText = "Укажите AE Title.";
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var created = 0;
        long firstPatientNumber = 0;
        string operationSummary = null;
        try
        {
            StartBusy("Создание заданий…");
            var modality = SelectModality?.Modality ?? "DX";
            var progress = new Progress<int>(percent => CurrentProgress = percent);
            using var client = OpenClient();
            await client.PingAsync();

            var existing = await client.ListAsync();
            firstPatientNumber = ResolveFirstPatientNumber(existing);

            var bodies = OrthancWorklistFactory.Create(
                WorkListCount.Value,
                modality,
                SetAeTitle,
                firstPatientNumber,
                options => GenerationOptions.ApplyTo(options));

            foreach (var body in bodies)
            {
                await client.CreateAsync(body);
                created++;
                ((IProgress<int>)progress).Report(created * 100 / bodies.Count);
            }

            await ReloadAsync(client);
            operationSummary = $"Добавлено: {created}.";
        }
        catch (Exception ex)
        {
            if (created > 0)
            {
                try
                {
                    await ReloadAsync();
                }
                catch (Exception reloadEx)
                {
                    UpdateText = $"Добавлено: {created}. Ошибка: {ex.Message} Список не обновлён: {reloadEx.Message}";
                    return;
                }
            }

            UpdateText = created > 0
                ? $"Добавлено: {created}. Ошибка: {ex.Message}"
                : $"Ошибка: {ex.Message}";
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
    private async Task AddSinglePatientAsync()
    {
        if (string.IsNullOrWhiteSpace(SingleLastName) || string.IsNullOrWhiteSpace(SingleFirstName))
        {
            UpdateText = "Укажите фамилию и имя.";
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        string operationSummary = null;
        try
        {
            StartBusy("Создание пациента…", percentShow: false);
            var modality = SingleModality?.Modality ?? "DX";
            using var client = OpenClient();
            await client.PingAsync();

            var patientId = SinglePatientId?.Trim();
            if (string.IsNullOrEmpty(patientId))
            {
                var existing = await client.ListAsync();
                var number = ResolveFirstPatientNumber(existing);
                patientId = OrthancWorklistFactory.PatientIdPrefix
                    + number.ToString("D7", CultureInfo.InvariantCulture);
            }

            var body = OrthancWorklistFactory.CreateOne(
                new OrthancWorklistPatientInput
                {
                    PatientId = patientId,
                    LastName = SingleLastName,
                    FirstName = SingleFirstName,
                    MiddleName = SingleMiddleName,
                    BirthDate = SingleBirthDate,
                    Sex = ToDicomSex(SingleSex),
                    Address = SingleAddress,
                    Telephone = SingleTelephone,
                    Comments = SingleComments,
                    Description = SingleDescription
                },
                modality,
                SetAeTitle);

            await client.CreateAsync(body);
            await ReloadAsync(client);
            ClearSinglePatient();
            operationSummary = $"Добавлен пациент {patientId}.";
        }
        catch (Exception ex)
        {
            UpdateText = $"Ошибка: {ex.Message}";
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
    private void ResetSinglePatient() => ClearSinglePatient();

    [RelayCommand]
    private void ResetGenerationOptions()
    {
        GenerationOptions.Reset();
        GenerationOptions.RefreshBirthdate();
    }

    [RelayCommand]
    private async Task RefreshWorkListAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        string operationSummary = null;
        try
        {
            StartBusy("Загрузка рабочего списка…", percentShow: false);
            await ReloadAsync();
            operationSummary = $"Записей: {Rows.Count}.";
        }
        catch (Exception ex)
        {
            UpdateText = $"Ошибка: {ex.Message}";
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
    private async Task DeleteFirstWorkListAsync()
    {
        var first = FirstVisible();
        if (first == null)
        {
            UpdateText = "В списке нет заданий. Нажмите Update WorkList.";
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        string operationSummary = null;
        try
        {
            StartBusy("Удаление задания…", percentShow: false);
            using var client = OpenClient();
            await client.DeleteAsync(first.Id);
            await ReloadAsync(client);
            operationSummary = $"Удалено задание {first.PatientId}.";
        }
        catch (Exception ex)
        {
            UpdateText = $"Ошибка: {ex.Message}";
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
    private async Task DeleteAllWorkListAsync()
    {
        var ids = Rows.Select(row => row.Id).Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
        if (ids.Count == 0)
        {
            UpdateText = "В списке нет заданий. Нажмите Update WorkList.";
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var deleted = 0;
        string operationSummary = null;
        try
        {
            StartBusy("Удаление заданий…");
            var progress = new Progress<int>(percent => CurrentProgress = percent);
            using var client = OpenClient();
            foreach (var id in ids)
            {
                await client.DeleteAsync(id);
                deleted++;
                ((IProgress<int>)progress).Report(deleted * 100 / ids.Count);
            }

            await ReloadAsync(client);
            operationSummary = $"Удалено: {deleted}.";
        }
        catch (Exception ex)
        {
            if (deleted > 0)
            {
                try
                {
                    await ReloadAsync();
                }
                catch (Exception reloadEx)
                {
                    UpdateText = $"Удалено: {deleted}. Ошибка: {ex.Message} Список не обновлён: {reloadEx.Message}";
                    return;
                }
            }

            UpdateText = deleted > 0
                ? $"Удалено: {deleted}. Ошибка: {ex.Message}"
                : $"Ошибка: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            StopBusy();
            if (operationSummary != null)
                UpdateText = $"{operationSummary} Время выполнения: {FormatTimeSpan(stopwatch.Elapsed)}";
        }
    }

    private async Task ReloadAsync(OrthancWorklistClient client = null)
    {
        var ownClient = client == null;
        client ??= OpenClient();
        try
        {
            var rows = await client.ListAsync();
            Rows = new ObservableCollection<OrthancWorklistEntry>(
                rows.OrderBy(row => row.PatientId ?? string.Empty, StringComparer.OrdinalIgnoreCase));
            ApplyFilter();
            SortByPatientId();
        }
        finally
        {
            if (ownClient)
                client.Dispose();
        }
    }

    private OrthancWorklistClient OpenClient() =>
        OrthancWorklistClient.Open(BaseUrl, Username, Password);

    /// <summary>
    /// Next MXE patient number: continues after the highest MXE ID currently in Orthanc.
    /// </summary>
    private long ResolveFirstPatientNumber(IEnumerable<OrthancWorklistEntry> existing)
    {
        var prefix = OrthancWorklistFactory.PatientIdPrefix;
        long max = 0;

        foreach (var entry in existing)
        {
            var id = entry.PatientId ?? string.Empty;
            if (!id.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            if (long.TryParse(id.Substring(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                && number > max)
            {
                max = number;
            }
        }

        return max + 1;
    }

    private static string ToDicomSex(string value)
    {
        if (value == "Man" || value == "M")
            return "M";
        if (value == "Female" || value == "F")
            return "F";
        if (value == "Other" || value == "O")
            return "O";
        return string.Empty;
    }

    private void ClearSinglePatient()
    {
        SinglePatientId = string.Empty;
        SingleLastName = string.Empty;
        SingleFirstName = string.Empty;
        SingleMiddleName = string.Empty;
        SingleBirthDate = null;
        SingleSex = null;
        SingleAddress = string.Empty;
        SingleTelephone = string.Empty;
        SingleComments = string.Empty;
        SingleDescription = string.Empty;
    }

    private OrthancWorklistEntry FirstVisible()
    {
        if (Rows.Count == 0)
            return null;

        var view = CollectionViewSource.GetDefaultView(Rows);
        return view?.Cast<OrthancWorklistEntry>().FirstOrDefault();
    }

    private void SortByPatientId()
    {
        var view = CollectionViewSource.GetDefaultView(Rows);
        if (view == null)
            return;

        view.SortDescriptions.Clear();
        view.SortDescriptions.Add(new SortDescription(
            nameof(OrthancWorklistEntry.PatientId),
            ListSortDirection.Ascending));
    }

    private void ApplyFilter()
    {
        var view = CollectionViewSource.GetDefaultView(Rows);
        if (view == null)
            return;

        var patientFilter = PatientIdFilter?.Trim();
        if (string.IsNullOrEmpty(patientFilter))
        {
            view.Filter = null;
            return;
        }

        view.Filter = item =>
        {
            var row = (OrthancWorklistEntry)item;
            return row.PatientId?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true;
        };
    }

    private void SaveConnection()
    {
        if (_loadingConnection)
            return;

        _optionsStore.SetOrthancConnection(BaseUrl, Username, Password);
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
}
