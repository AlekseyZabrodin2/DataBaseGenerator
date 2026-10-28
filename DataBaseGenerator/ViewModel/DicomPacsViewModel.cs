using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        public string FolderPathFull
        {
            get => _folderPathFull;
            set
            {
                SetProperty(ref _folderPathFull, value);
                BuildShortPath(_folderPathFull);
            }
        }

        private readonly GenerationOptionsStore _optionsStore;


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

            var dicomFiles = Directory.GetFiles(FolderPathFull, "*.*");

            var fileCount = 0;

            if (dicomFiles.Length == 0)
            {
                ServerOutputText = "Файлов не найдено.";
                return;
            }

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
                                Interlocked.Increment(ref fileCount);

                                var percent = fileCount * 100 / dicomFiles.Length;
                                scanProgress.Report((percent, $"DICOM файл {fileCount} из {dicomFiles.Length} отправлен успешно."));
                            }
                        };

                        requests.Add(storeRequest);
                    }
                    await client.AddRequestsAsync(requests);
                    await client.SendAsync(cancellationToken, cancellationMode: DicomClientCancellationMode.ImmediatelyReleaseAssociation);
                }
            }
            catch (OperationCanceledException)
            {
                ServerOutputText = $"Отправка DICOM файла {fileCount} была отменена.";
                return;
            }
            catch (Exception ex)
            {
                ServerOutputText = $"Ошибка: {ex.Message}";
                return;
            }
            finally
            {
                stopwatch.Stop();
                var elapsedTime = stopwatch.Elapsed;
                var timeString = FormatTimeSpan(elapsedTime);

                IsBusy = false;

                ServerOutputText = $"DICOM файл {fileCount} отправлен успешно. Время выполнения: {timeString}";
            }
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
