using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.Data
{
    public sealed class LiteDbReadOnlyBrowser : IDisposable
    {
        private readonly LiteDatabase _database;

        public LiteDbReadOnlyBrowser(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь к файлу не задан.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден. Новый файл не создаётся.", path);

            DatabasePath = path;
            _database = new LiteDatabase(new ConnectionString
            {
                Filename = path,
                ReadOnly = true
            });
        }

        public string DatabasePath { get; }

        public List<string> GetCollectionNames()
        {
            return new List<string>(_database.GetCollectionNames());
        }

        public IReadOnlyList<IReadOnlyDictionary<string, string>> ReadCollection(string collectionName)
        {
            return LiteDbDocumentReader.Read(_database.GetCollection(collectionName).FindAll());
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
