using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.UI.Wpf.Model;
using DataBaseGenerator.UI.Wpf.ViewModel.Options;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using Microsoft.Win32;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class DicomPacsViewModel : ObservableObject
    {
        private string _folderPathFull = string.Empty;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isUpdatingSelection;
        private bool _areAllStudiesSelected;
        private readonly GenerationOptionsStore _optionsStore;

        [ObservableProperty]
        public partial string ServerIp { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ServerPort { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CallingAe { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CalledAe { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ServerOutputText { get; set; }

        [ObservableProperty]
        public partial string FolderPathShort { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string BusyMessage { get; set; }

        [ObservableProperty]
        public partial int CurrentProgress { get; set; }

        public ObservableCollection<DicomStudyPreview> PreviewStudies { get; } = [];

        [ObservableProperty]
        public partial int PreviewStudyCount { get; set; }

        [ObservableProperty]
        public partial int PreviewFileCount { get; set; }

        [ObservableProperty]
        public partial int SelectedFileCount { get; set; }

        public string FolderPathFull
        {
            get => _folderPathFull;
            set
            {
                if (SetProperty(ref _folderPathFull, value))
                {
                    BuildShortPath(_folderPathFull);
                    PreviewStudies.Clear();
                    PreviewStudyCount = 0;
                    PreviewFileCount = 0;
                    AreAllStudiesSelected = false;
                }
            }
        }

        public bool AreAllStudiesSelected
        {
            get => _areAllStudiesSelected;
            set
            {
                if (!SetProperty(ref _areAllStudiesSelected, value))
                    return;

                try
                {
                    _isUpdatingSelection = true;

                    foreach (var study in PreviewStudies)
                        study.IsSelected = value;
                }
                finally
                {
                    _isUpdatingSelection = false;

                    SelectedFileCount = PreviewStudies
                        .Where(study => study.IsSelected)
                        .Sum(study => study.FileCount);
                }
            }
        }



        public DicomPacsViewModel(GenerationOptionsStore optionsStore)
        {
            _optionsStore = optionsStore;

            InitialiseServerConnectingProperty();
        }


        private void InitialiseServerConnectingProperty()
        {
            ServerIp = _optionsStore.PacsServerIp;
            ServerPort = _optionsStore.PacsServerPort;
            CallingAe = _optionsStore.PacsCallingAe;
            CalledAe = _optionsStore.PacsCalledAe;
            FolderPathFull = _optionsStore.PacsFolderPath;
        }


        [RelayCommand]
        private async Task SendDicomImage()
        {
            SavePacsConfig();

            var stopwatch = Stopwatch.StartNew();

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            if (!int.TryParse(ServerPort, out var port))
            {
                ServerOutputText = "Некорректный порт.";
                return;
            }

            var client = DicomClientFactory.Create(ServerIp, port, false, CallingAe, CalledAe);
            client.ClientOptions.MaximumNumberOfRequestsPerAssociation = 500;

            //var dicomFiles = Directory.GetFiles(FolderPathFull, "*.*");

            if (PreviewStudies.Count == 0)
            {
                ServerOutputText = "Сначала выполните предпросмотр исследований.";
                return;
            }

            var dicomFiles = PreviewStudies
                .Where(study => study.IsSelected)
                .SelectMany(study => study.FilePaths)
                .ToArray();

            if (dicomFiles.Length == 0)
            {
                ServerOutputText = "Выберите хотя бы одно исследование.";
                return;
            }

            var fileCount = 0;

            if (dicomFiles.Length == 0)
            {
                ServerOutputText = "Файлов не найдено.";
                return;
            }

            var completed = false;

            try
            {
                IsBusy = true;

                var scanProgress = CreateProgress();

                const int batchSize = 500;

                for (var i = 0; i < dicomFiles.Length; i += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var batch = dicomFiles.Skip(i).Take(batchSize);
                    var requests = new List<DicomRequest>();

                    foreach (var filePath in batch)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var dicomFile = await DicomFile.OpenAsync(filePath);
                        var storeRequest = new DicomCStoreRequest(dicomFile);

                        storeRequest.OnResponseReceived += (_, response) =>
                        {
                            if (response.Status.State == DicomState.Success)
                            {
                                var count = Interlocked.Increment(ref fileCount);
                                var percent = count * 100 / dicomFiles.Length;

                                scanProgress.Report((
                                    percent,
                                    $"Отправлено {count} из {dicomFiles.Length} файлов."));
                            }
                        };

                        requests.Add(storeRequest);
                    }

                    await client.AddRequestsAsync(requests);
                    await client.SendAsync(
                        cancellationToken,
                        cancellationMode: DicomClientCancellationMode.ImmediatelyReleaseAssociation);
                }

                completed = true;
            }
            catch (OperationCanceledException)
            {
                ServerOutputText = $"Отправка отменена. Успешно отправлено файлов: {fileCount}.";
                return;
            }
            catch (Exception ex)
            {
                ServerOutputText = $"Ошибка отправки: {ex.Message}";
                return;
            }
            finally
            {
                stopwatch.Stop();
                IsBusy = false;

                if (completed)
                {
                    ServerOutputText =
                        $"Отправлено файлов: {fileCount} из {dicomFiles.Length}. " +
                        $"Время выполнения: {FormatTimeSpan(stopwatch.Elapsed)}";
                }
            }
        }


        [RelayCommand]
        private async Task PreviewStudiesAsync()
        {
            if (string.IsNullOrWhiteSpace(FolderPathFull) ||
                !Directory.Exists(FolderPathFull))
            {
                ServerOutputText = "Выберите существующую директорию.";
                return;
            }

            IsBusy = true;
            BusyMessage = "Сканирование DICOM-файлов...";
            PreviewStudies.Clear();
            PreviewStudyCount = 0;
            PreviewFileCount = 0;
            AreAllStudiesSelected = false;

            try
            {
                var folderPath = FolderPathFull;

                var (studies, invalidFileCount) = await Task.Run(async () =>
                {
                    var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

                    var foundStudies = new Dictionary<string, DicomStudyPreview>();
                    var invalidCount = 0;

                    foreach (var filePath in files)
                    {
                        try
                        {
                            var dicomFile = await DicomFile.OpenAsync(filePath);
                            var dataset = dicomFile.Dataset;

                            var studyUid = dataset.GetSingleValueOrDefault<string>(
                                DicomTag.StudyInstanceUID, string.Empty);

                            // Файл без StudyInstanceUID не объединяем с другими.
                            var key = string.IsNullOrWhiteSpace(studyUid)
                                ? $"__NO_UID__{filePath}"
                                : studyUid;

                            if (foundStudies.TryGetValue(key, out var existingStudy))
                            {
                                existingStudy.FilePaths.Add(filePath);
                                continue;
                            }

                            var patientName = dataset
                                .GetSingleValueOrDefault(DicomTag.PatientName, string.Empty)
                                .Replace('^', ' ')
                                .Trim();

                            var studyDate = dataset.GetSingleValueOrDefault(DicomTag.StudyDate, string.Empty);
                            studyDate = DateTime.TryParseExact(studyDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                                    ? date.ToString("dd.MM.yyyy")
                                    : studyDate;

                            foundStudies[key] = new DicomStudyPreview
                            {
                                StudyInstanceUid = studyUid,
                                PatientName = patientName,
                                PatientId = dataset.GetSingleValueOrDefault<string>(DicomTag.PatientID, string.Empty),
                                StudyDate = studyDate,
                                StudyDescription = dataset.GetSingleValueOrDefault<string>(DicomTag.StudyDescription, string.Empty),
                                FilePaths = [filePath],
                                IsSelected = true
                            };
                        }
                        catch
                        {
                            invalidCount++;
                        }
                    }

                    return (foundStudies.Values.ToList(), invalidCount);
                });

                foreach (var study in studies)
                {
                    study.PropertyChanged += PreviewStudy_PropertyChanged;
                    PreviewStudies.Add(study);
                }

                PreviewStudyCount = PreviewStudies.Count;
                PreviewFileCount = PreviewStudies.Sum(study => study.FileCount);
                AreAllStudiesSelected = PreviewStudies.Count > 0;

                SelectedFileCount = PreviewStudies
                    .Where(study => study.IsSelected)
                    .Sum(study => study.FileCount);

                ServerOutputText =
                    $"Найдено исследований: {PreviewStudyCount}. " +
                    $"DICOM-файлов: {PreviewFileCount}. " +
                    $"Пропущено файлов: {invalidFileCount}.";
            }
            catch (Exception ex)
            {
                ServerOutputText = $"Ошибка предпросмотра: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void PreviewStudy_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(DicomStudyPreview.IsSelected) ||
                _isUpdatingSelection)
            {
                return;
            }
            if (e.PropertyName == nameof(DicomStudyPreview.IsSelected))
            {
                if (!_isUpdatingSelection)
                    AreAllStudiesSelected = PreviewStudies.All(study => study.IsSelected);

                SelectedFileCount = PreviewStudies
                    .Where(study => study.IsSelected)
                    .Sum(study => study.FileCount);
            }

            AreAllStudiesSelected =
                PreviewStudies.Count > 0 &&
                PreviewStudies.All(study => study.IsSelected);
        }


        [RelayCommand]
        private void CancaelSendingImage()
        {
            _cancellationTokenSource?.Cancel();
        }

        [RelayCommand]
        private void SavePacsConfigurations()
        {
            SavePacsConfig();
            ServerOutputText = "Configurations are saved";
        }

        [RelayCommand]
        public void BrowseDatabase()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Выберите путь к папке"
            };

            if (dialog.ShowDialog() == true)
            {
                FolderPathFull = dialog.FolderName;
                SavePacsConfig();
            }
        }

        [RelayCommand]
        public async Task EchoAsync()
        {
            if (string.IsNullOrWhiteSpace(ServerPort) || string.IsNullOrWhiteSpace(ServerPort))
            {
                ServerOutputText = "Please enter Host or Port.";
                return;
            }

            ServerOutputText = "Connecting ...";

            try
            {
                if (!int.TryParse(ServerPort, out var port))
                {
                    ServerOutputText = "Некорректный порт.";
                    return;
                }

                var client = DicomClientFactory.Create(ServerIp, port, false, CallingAe, CalledAe);

                if (client == null)
                {
                    ServerOutputText = "Не удается создать клиент PACS.";
                    return;
                }

                DicomStatus responseStatus = null;
                var echoRequest = new DicomCEchoRequest();

                echoRequest.OnResponseReceived += (request, response) =>
                {
                    responseStatus = response.Status;
                };

                await client.AddRequestAsync(echoRequest);
                await client.SendAsync();

                if (responseStatus?.State == DicomState.Success)
                {
                    ServerOutputText = $"PacS - [ {client.CalledAe} ] connected successful! ({client.Host} : {client.Port})";
                }
                else
                {
                    ServerOutputText = $"PACS вернул статус: {responseStatus}";
                }
            }
            catch (Exception ex)
            {
                ServerOutputText = $"error: {ex.Message}";
            }
        }

        private void SavePacsConfig()
        {
            _optionsStore.SetPacsServerIp(ServerIp);
            _optionsStore.SetPacsServerPort(ServerPort);
            _optionsStore.SetPacsCallingAe(CallingAe);
            _optionsStore.SetPacsCalledAe(CalledAe);
            _optionsStore.SetPacsFolderPath(FolderPathFull);
        }

        private string BuildShortPath(string fullPath, int keepFolders = 3)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return string.Empty;

            // Нормализуем разделители и убираем хвостовые '\'
            var normalized = fullPath.Replace('/', '\\').TrimEnd('\\');

            // Для коротких путей ничего не делаем
            var parts = normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= keepFolders + 1) // +1 = имя файла
            {
                FolderPathShort = normalized;
                return normalized;
            }

            var fileName = parts[^1];
            var startIndex = Math.Max(0, parts.Length - (keepFolders + 1));
            var tail = string.Join("\\", parts[startIndex..]);

            // Если есть диск (C:) или UNC, префикс всё равно делаем через "..."
            FolderPathShort = $@"...\{tail}";

            return FolderPathShort;
        }

        private string FormatTimeSpan(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return $"{time.Hours} ч {time.Minutes} мин {time.Seconds} сек";
            }
            if (time.TotalMinutes >= 1)
            {
                return $"{time.Minutes} мин {time.Seconds} сек";
            }
            return $"{time.TotalSeconds:F1} сек";
        }

        private IProgress<(int Percent, string Message)> CreateProgress()
        {
            return new Progress<(int Percent, string Message)>(percent =>
            {
                CurrentProgress = percent.Percent;
                BusyMessage = percent.Message;
            });
        }
    }
}
