using System.Collections.Generic;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.Data
{
    public static class LiteDbDocumentReader
    {
        public static IReadOnlyList<IReadOnlyDictionary<string, string>> Read(IEnumerable<BsonDocument> documents)
        {
            var rows = new List<IReadOnlyDictionary<string, string>>();

            foreach (var document in documents)
                rows.Add(ToRow(document));

            return rows;
        }

        private static Dictionary<string, string> ToRow(BsonDocument document)
        {
            var row = new Dictionary<string, string>(document.Keys.Count);

            foreach (var key in document.Keys)
            {
                var column = key == "_id" ? "Id" : key;
                row[column] = Format(document[key]);
            }

            return row;
        }

        private static string Format(BsonValue value)
        {
            if (value == null || value.IsNull)
                return string.Empty;

            if (value.IsObjectId)
                return value.AsObjectId.ToString();

            if (value.IsDateTime)
                return value.AsDateTime.ToString("dd.MM.yyyy HH:mm:ss");

            return value.ToString();
        }
    }
}
