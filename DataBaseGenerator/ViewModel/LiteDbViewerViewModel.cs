using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.LiteDbGenerator.Data;
using Microsoft.Win32;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class LiteDbViewerViewModel : ObservableObject, IDisposable
    {
        private static readonly TimeSpan AutoRefreshInterval = TimeSpan.FromSeconds(10);

        private readonly object _readLock = new();
        private readonly DispatcherTimer _autoRefreshTimer;
        private LiteDbReadOnlyBrowser _external;
        private string _externalPath;
        private int _loadVersion;
        private int _refreshRunning;
        private int _slowOperations;
        private int _busyStamp;
        private bool _pageActive;
        private string _statusCollection;
        private int _statusTotal;

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

        [ObservableProperty]
        public partial bool AutoRefresh { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string BusyMessage { get; set; }

        [ObservableProperty]
        public partial string FilterText { get; set; }

        partial void OnFilterTextChanged(string value)
        {
            ApplyFilter();
        }

        public LiteDbViewerViewModel()
        {
            Files = new ObservableCollection<LiteDbViewerFile>();
            Collections = new ObservableCollection<string>();
            _autoRefreshTimer = new DispatcherTimer { Interval = AutoRefreshInterval };
            _autoRefreshTimer.Tick += (_, _) => _ = RefreshFromTimerAsync();
        }

        public void SetPageActive(bool active)
        {
            _pageActive = active;
            if (!active)
                AutoRefresh = false;

            UpdateAutoRefreshTimer();
        }

        partial void OnAutoRefreshChanged(bool value)
        {
            UpdateAutoRefreshTimer();
        }

        private void UpdateAutoRefreshTimer()
        {
            if (_pageActive && AutoRefresh)
            {
                if (!_autoRefreshTimer.IsEnabled)
                    _autoRefreshTimer.Start();
                return;
            }

            _autoRefreshTimer.Stop();
        }

        private async Task RefreshFromTimerAsync()
        {
            if (!_pageActive || !AutoRefresh)
                return;

            await RefreshAsync();
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
            _ = LoadCollectionsAsync(value, null);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (Interlocked.Exchange(ref _refreshRunning, 1) == 1)
                return;

            try
            {
                var file = SelectedFile;
                if (file == null)
                {
                    LoadOpenFiles();
                    return;
                }

                var collection = SelectedCollection;
                _loadVersion++;

                try
                {
                    Reopen(file);
                }
                catch (Exception ex)
                {
                    StatusText = ex.Message;
                    return;
                }

                await LoadCollectionsAsync(file, collection);
            }
            finally
            {
                Interlocked.Exchange(ref _refreshRunning, 0);
            }
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

        private async Task LoadCollectionsAsync(LiteDbViewerFile file, string keepCollection)
        {
            var version = ++_loadVersion;
            Collections.Clear();
            SelectedCollection = null;
            Rows = null;

            if (file == null)
                return;

            StatusText = "Чтение коллекций...";
            BeginSlowOperation();

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

                var next = names.FirstOrDefault(name => string.Equals(name, keepCollection, StringComparison.OrdinalIgnoreCase))
                    ?? names.FirstOrDefault();

                if (next != null)
                    SelectedCollection = next;
            }
            catch (Exception ex)
            {
                if (version != _loadVersion)
                    return;

                StatusText = ex.Message;
            }
            finally
            {
                EndSlowOperation();
            }
        }

        private async Task LoadRowsAsync(LiteDbViewerFile file, string collectionName)
        {
            var version = _loadVersion;
            Rows = null;

            if (file == null || string.IsNullOrWhiteSpace(collectionName))
                return;

            StatusText = "Чтение таблицы...";
            BeginSlowOperation();

            try
            {
                var rows = await Task.Run(() => ReadRows(file, collectionName));
                if (version != _loadVersion || SelectedCollection != collectionName)
                    return;

                _statusCollection = collectionName;
                _statusTotal = rows.Count;
                Rows = ToView(rows);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                if (version != _loadVersion)
                    return;

                StatusText = ex.Message;
            }
            finally
            {
                EndSlowOperation();
            }
        }

        private void BeginSlowOperation()
        {
            if (Interlocked.Increment(ref _slowOperations) != 1)
                return;

            var stamp = Volatile.Read(ref _busyStamp);
            _ = ShowBusyIfSlowAsync(stamp);
        }

        private void EndSlowOperation()
        {
            if (Interlocked.Decrement(ref _slowOperations) > 0)
                return;

            Interlocked.Exchange(ref _slowOperations, 0);
            Interlocked.Increment(ref _busyStamp);
            IsBusy = false;
            BusyMessage = string.Empty;
        }

        private async Task ShowBusyIfSlowAsync(int stamp)
        {
            await Task.Delay(400);

            if (stamp != Volatile.Read(ref _busyStamp))
                return;

            if (Volatile.Read(ref _slowOperations) <= 0)
                return;

            BusyMessage = "Обновление...";
            IsBusy = true;
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

        private void Reopen(LiteDbViewerFile file)
        {
            if (!File.Exists(file.Path))
                throw new FileNotFoundException("Файл не найден. Новый файл не создаётся.", file.Path);

            lock (_readLock)
            {
                switch (file.Kind)
                {
                    case LiteDbViewerFileKind.Shared:
                        App.UpdateSharedStorage(file.Path, true);
                        break;
                    case LiteDbViewerFileKind.Planned:
                        App.UpdatePlannedStorage(file.Path, true);
                        break;
                    default:
                        _external?.Dispose();
                        _external = null;
                        _externalPath = null;
                        break;
                }
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

        private void ApplyFilter()
        {
            if (Rows?.Table == null)
                return;

            try
            {
                Rows.RowFilter = BuildRowFilter(Rows.Table, FilterText);
            }
            catch (Exception)
            {
                Rows.RowFilter = string.Empty;
            }

            if (string.IsNullOrEmpty(_statusCollection))
                return;

            var visible = Rows.Count;
            StatusText = string.IsNullOrWhiteSpace(FilterText) || visible == _statusTotal
                ? $"{_statusCollection}: {_statusTotal}"
                : $"{_statusCollection}: {visible} из {_statusTotal}";
        }

        private static string BuildRowFilter(DataTable table, string text)
        {
            var filter = text?.Trim();
            if (string.IsNullOrEmpty(filter) || table.Columns.Count == 0)
                return string.Empty;

            var escaped = EscapeLike(filter);
            var parts = new List<string>(table.Columns.Count);

            foreach (DataColumn column in table.Columns)
            {
                var name = column.ColumnName.Replace("]", "]]");
                parts.Add($"Convert([{name}], 'System.String') LIKE '%{escaped}%'");
            }

            return string.Join(" OR ", parts);
        }

        private static string EscapeLike(string value)
        {
            var builder = new StringBuilder(value.Length);

            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\'':
                        builder.Append("''");
                        break;
                    case '%':
                    case '*':
                    case '[':
                    case ']':
                        builder.Append('[').Append(ch).Append(']');
                        break;
                    default:
                        builder.Append(ch);
                        break;
                }
            }

            return builder.ToString();
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
