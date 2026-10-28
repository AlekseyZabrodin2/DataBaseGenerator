using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.UI.Wpf.Model;
using FellowOakDicom;
using Microsoft.Win32;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class DicomReaderViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


        public ObservableCollection<string> DicomFilesCollection { get; } = new();

        [ObservableProperty]
        public partial ObservableCollection<DicomTagInfo> DicomTags { get; set; } = new();

        [ObservableProperty]
        public partial DicomPatientInfo PatientInfo { get; set; } = new();

        [ObservableProperty]
        public partial string SelectedFile { get; set; }

        [ObservableProperty]
        public partial string DatabasePathShort { get; set; }

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        public ICollectionView FilteredDicomTags { get; }


        private string _databasePath = string.Empty;

        public string DatabasePath
        {
            get => _databasePath;
            set
            {
                SetProperty(ref _databasePath, value);
                BuildShortPath(_databasePath);
            }
        }


        public DicomReaderViewModel()
        {
            FilteredDicomTags = CollectionViewSource.GetDefaultView(DicomTags);
            FilteredDicomTags.Filter = FilterDicomTag;
        }



        partial void OnSelectedFileChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            SearchText = string.Empty;
            ReadAllDicomTags(value);
        }

        partial void OnSearchTextChanged(string value)
        {
            FilteredDicomTags.Refresh();
        }


        [RelayCommand]
        public void BrowseDicomFilesFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Выберите DICOM-файл"
            };

            if (dialog.ShowDialog() == true)
            {
                DicomFilesCollection.Clear();
                DicomTags.Clear();
                PatientInfo = null;

                DatabasePath = dialog.FolderName;

                var fileName = Directory
                    .EnumerateFiles(DatabasePath, "*", SearchOption.AllDirectories)
                    .Select(Path.GetFileName)
                    .Where(name => name != null)
                    .Distinct();

                foreach (var file in fileName)
                {
                    DicomFilesCollection.Add(file);
                }
            }
        }

        [RelayCommand]
        private void RefreshFiles()
        {
            if (string.IsNullOrWhiteSpace(DatabasePath) || !Directory.Exists(DatabasePath))
            {
                return;
            }

            DicomFilesCollection.Clear();

            var fileName = Directory
                    .EnumerateFiles(DatabasePath, "*", SearchOption.AllDirectories)
                    .Select(Path.GetFileName)
                    .Where(name => name != null)
                    .Distinct();

            foreach (var file in fileName)
            {
                DicomFilesCollection.Add(file);
            }
        }

        private void ReadAllDicomTags(string filePath)
        {
            try
            {
                var dicomFile = DicomFile.Open(Path.Combine(DatabasePath, filePath));

                DicomTags.Clear();

                AddDicomTags(dicomFile.FileMetaInfo, "File Meta Information");

                AddDicomTags(dicomFile.Dataset, string.Empty);

                ReadPatientInfo(dicomFile.Dataset);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to read DICOM file: {filePath}");
            }
        }

        private void AddDicomTags(DicomDataset dataset, string parentPath = "", int level = 0)
        {
            foreach (var item in dataset)
            {
                var tagDescription = item.Tag.DictionaryEntry.Name;
                var tagPath = string.IsNullOrEmpty(parentPath)
                    ? tagDescription
                    : $"{parentPath} / {tagDescription}";

                if (item is DicomSequence sequence)
                {
                    DicomTags.Add(new DicomTagInfo
                    {
                        GroupElement = item.Tag.ToString(),
                        TagDescription = $"{tagPath} [SQ]",
                        Value = $"Items: {sequence.Items.Count}",
                        Level = level
                    });

                    for (var i = 0; i < sequence.Items.Count; i++)
                    {
                        AddDicomTags(
                            sequence.Items[i],
                            $"{tagPath}[{i}]",
                            level + 1);
                    }

                    continue;
                }

                DicomTags.Add(new DicomTagInfo
                {
                    GroupElement = item.Tag.ToString(),
                    TagDescription = tagPath,
                    Value = GetDicomTagValue(item),
                    Level = level
                });
            }
        }

        private string GetDicomTagValue(DicomItem item)
        {
            if (item is not DicomElement element)
                return "[Unsupported item]";

            try
            {
                if (element.Tag == DicomTag.PixelData)
                    return $"[Pixel Data; {element.Buffer.Size} bytes]";

                return string.Join(", ", element.Get<string[]>());
            }
            catch (Exception ex)
            {
                _logger.Debug(
                    $"Cannot read tag {item.Tag} as text: {ex.Message}");

                return $"[Binary or non-text value; VR={item.ValueRepresentation.Code}]";
            }
        }

        private void ReadPatientInfo(DicomDataset dataset)
        {
            PatientInfo = new DicomPatientInfo
            {
                PatientName = GetTagValue(dataset, DicomTag.PatientName),
                PatientId = GetTagValue(dataset, DicomTag.PatientID),
                BirthDate = FormatDate(GetTagValue(dataset, DicomTag.PatientBirthDate)),
                Sex = GetTagValue(dataset, DicomTag.PatientSex),
                Age = GetTagValue(dataset, DicomTag.PatientAge),
                Weight = GetTagValue(dataset, DicomTag.PatientWeight),
                Address = GetTagValue(dataset, DicomTag.PatientAddress),

                StudyDate = FormatDate(GetTagValue(dataset, DicomTag.StudyDate)),
                StudyTime = FormatTime(GetTagValue(dataset, DicomTag.StudyTime)),
                StudyId = GetTagValue(dataset, DicomTag.StudyID),
                Modality = GetTagValue(dataset, DicomTag.Modality),
                StudyDescription = GetTagValue(dataset, DicomTag.StudyDescription),

                SeriesDate = FormatDate(GetTagValue(dataset, DicomTag.SeriesDate)),
                SeriesTime = FormatTime(GetTagValue(dataset, DicomTag.SeriesTime)),
                SeriesDescription = GetTagValue(dataset, DicomTag.SeriesDescription)
            };
        }

        private static string GetTagValue(DicomDataset dataset, DicomTag tag)
        {
            return dataset.GetSingleValueOrDefault(tag, string.Empty);
        }

        private static string FormatDate(string value)
        {
            return DateTime.TryParseExact(
                value,
                "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var date)
                ? date.ToString("dd-MMMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)
                : value;
        }

        private static string FormatTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return DateTime.TryParseExact(
                value.Split('.')[0],
                new[] { "HHmmss", "HHmm", "HH" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var time)
                ? time.ToString("HH:mm:ss")
                : value;
        }

        private bool FilterDicomTag(object item)
        {
            if (item is not DicomTagInfo tag)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            return tag.GroupElement.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || tag.TagDescription.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || tag.Value.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
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
                DatabasePathShort = normalized;
                return normalized;
            }

            var fileName = parts[^1];
            var startIndex = Math.Max(0, parts.Length - (keepFolders + 1));
            var tail = string.Join("\\", parts[startIndex..]);

            // Если есть диск (C:) или UNC, префикс всё равно делаем через "..."
            DatabasePathShort = $@"...\{tail}";

            return DatabasePathShort;
        }
    }
}