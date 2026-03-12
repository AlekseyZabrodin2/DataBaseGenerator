using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.Mappings
{
    internal static class ObjectIdMapper
    {
        public static bool TryParse(string? value, out ObjectId id)
        {
            id = ObjectId.Empty;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                id = new ObjectId(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string? ToString(ObjectId id)
        {
            return id == ObjectId.Empty ? null : id.ToString();
        }
    }
}
