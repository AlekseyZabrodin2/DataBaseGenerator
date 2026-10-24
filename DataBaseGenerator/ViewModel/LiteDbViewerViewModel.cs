using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.LiteDbGenerator.Data;
using Microsoft.Win32;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class LiteDbViewerViewModel : ObservableObject, IDisposable
    {
        private readonly object _readLock = new();
        private LiteDbReadOnlyBrowser _external;
        private string _externalPath;
        private int _loadVersion;

        [ObservableProperty]
        public partial ObservableCollection<LiteDbViewerFile> Files { get; set; }

        [ObservableProperty]
        public partial LiteDbViewerFile SelectedFile { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<string> Collections { get; set; }

        [ObservableProperty]
        public partial string SelectedCollection { get; set; }

        [ObservableProperty]
        public partial DataView Rows { get; set; }

        [ObservableProperty]
        public partial string StatusText { get; set; }

        public LiteDbViewerViewModel()
        {
            Files = new ObservableCollection<LiteDbViewerFile>();
            Collections = new ObservableCollection<string>();
        }

        public void LoadOpenFiles()
        {
            _loadVersion++;
            Files.Clear();
            Rows = null;
            Collections.Clear();
            SelectedCollection = null;

            var sharedPath = App.SharedStorage?.DatabasePath;
            var plannedPath = App.PlannedStorage?.DatabasePath;

            if (!string.IsNullOrWhiteSpace(sharedPath))
            {
                Files.Add(new LiteDbViewerFile
                {
                    Kind = LiteDbViewerFileKind.Shared,
                    Path = sharedPath,
                    Title = Path.GetFileName(sharedPath)
                });
            }

            if (!string.IsNullOrWhiteSpace(plannedPath) && !SamePath(sharedPath, plannedPath))
            {
                Files.Add(new LiteDbViewerFile
                {
                    Kind = LiteDbViewerFileKind.Planned,
                    Path = plannedPath,
                    Title = Path.GetFileName(plannedPath)
                });
            }

            SelectedFile = Files.FirstOrDefault();
            if (SelectedFile == null)
                StatusText = "Нет открытых файлов LiteDB.";
        }

        public void ReleaseExternal()
        {
            _loadVersion++;

            lock (_readLock)
            {
                _external?.Dispose();
                _external = null;
                _externalPath = null;
            }
        }

        partial void OnSelectedFileChanged(LiteDbViewerFile value)
        {
            _ = LoadCollectionsAsync(value);
        }

        partial void OnSelectedCollectionChanged(string value)
        {
            _ = LoadRowsAsync(SelectedFile, value);
        }

        [RelayCommand]
        private void BrowseFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "LiteDB files (*.db)|*.db|All files (*.*)|*.*",
                Title = "Выберите файл LiteDB"
            };

            if (dialog.ShowDialog() != true)
                return;

            if (!File.Exists(dialog.FileName))
            {
                StatusText = "Файл не найден. Новый файл не создаётся.";
                return;
            }

            var existing = Files.FirstOrDefault(file => SamePath(file.Path, dialog.FileName));
            if (existing != null)
            {
                SelectedFile = existing;
                return;
            }

            var item = new LiteDbViewerFile
            {
                Kind = LiteDbViewerFileKind.External,
                Path = dialog.FileName,
                Title = Path.GetFileName(dialog.FileName)
            };

            Files.Add(item);
            SelectedFile = item;
        }

        private async Task LoadCollectionsAsync(LiteDbViewerFile file)
        {
            var version = ++_loadVersion;
            Collections.Clear();
            SelectedCollection = null;
            Rows = null;

            if (file == null)
                return;

            StatusText = "Чтение коллекций...";

            try
            {
                var names = await Task.Run(() => ReadCollectionNames(file));
                if (version != _loadVersion)
                    return;

                foreach (var name in names.OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
                    Collections.Add(name);

                StatusText = names.Count == 0
                    ? "В файле нет коллекций."
                    : file.Path;

                if (Collections.Count > 0)
                    SelectedCollection = Collections[0];
            }
            catch (Exception ex)
            {
                if (version != _loadVersion)
                    return;

                StatusText = ex.Message;
            }
        }

        private async Task LoadRowsAsync(LiteDbViewerFile file, string collectionName)
        {
            var version = _loadVersion;
            Rows = null;

            if (file == null || string.IsNullOrWhiteSpace(collectionName))
                return;

            StatusText = "Чтение таблицы...";

            try
            {
                var rows = await Task.Run(() => ReadRows(file, collectionName));
                if (version != _loadVersion || SelectedCollection != collectionName)
                    return;

                Rows = ToView(rows);
                StatusText = $"{collectionName}: {rows.Count}";
            }
            catch (Exception ex)
            {
                if (version != _loadVersion)
                    return;

                StatusText = ex.Message;
            }
        }

        private List<string> ReadCollectionNames(LiteDbViewerFile file)
        {
            lock (_readLock)
            {
                var names = file.Kind switch
                {
                    LiteDbViewerFileKind.Shared => App.SharedStorage.GetCollectionNames(),
                    LiteDbViewerFileKind.Planned => App.PlannedStorage.GetCollectionNames(),
                    _ => External(file.Path).GetCollectionNames()
                };

                return names
                    .Where(name => !string.IsNullOrWhiteSpace(name) && !name.StartsWith('$'))
                    .ToList();
            }
        }

        private IReadOnlyList<IReadOnlyDictionary<string, string>> ReadRows(LiteDbViewerFile file, string collectionName)
        {
            lock (_readLock)
            {
                return file.Kind switch
                {
                    LiteDbViewerFileKind.Shared => App.SharedStorage.ReadCollection(collectionName),
                    LiteDbViewerFileKind.Planned => App.PlannedStorage.ReadCollection(collectionName),
                    _ => External(file.Path).ReadCollection(collectionName)
                };
            }
        }

        private LiteDbReadOnlyBrowser External(string path)
        {
            if (_external != null && SamePath(_externalPath, path))
                return _external;

            _external?.Dispose();
            _external = new LiteDbReadOnlyBrowser(path);
            _externalPath = path;
            return _external;
        }

        private static bool SamePath(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
                return false;

            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
        }

        private static DataView ToView(IReadOnlyList<IReadOnlyDictionary<string, string>> rows)
        {
            var table = new DataTable();
            var columns = new List<string>();

            foreach (var row in rows)
            {
                foreach (var key in row.Keys)
                {
                    if (!columns.Contains(key))
                        columns.Add(key);
                }
            }

            if (columns.Remove("Id"))
                columns.Insert(0, "Id");

            foreach (var column in columns)
                table.Columns.Add(column);

            foreach (var row in rows)
            {
                var dataRow = table.NewRow();
                foreach (var column in columns)
                    dataRow[column] = row.TryGetValue(column, out var value) ? value : string.Empty;

                table.Rows.Add(dataRow);
            }

            return table.DefaultView;
        }

        public void Dispose()
        {
            ReleaseExternal();
        }
    }

    public enum LiteDbViewerFileKind
    {
        Shared,
        Planned,
        External
    }

    public sealed class LiteDbViewerFile
    {
        public LiteDbViewerFileKind Kind { get; set; }

        public string Path { get; set; }

        public string Title { get; set; }
    }
}
