using System;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace DataBaseGenerator.Core.DicomGenerator.DicomGeneratorModels
{
    public static class UidGenerator
    {
        private const string _manufacturerUid = "1.2.826.0.1.3680043.2.564";

        
        public static string GenerateStudyUid()
        {
            return GenerateUid();
        }

        public static string GenerateSeriesUid()
        {
            return GenerateUid();
        }

        public static string GenerateImageUid()
        {
            return GenerateUid();
        }

        public static string GenerateTargetUid()
        {
            return GenerateUid();
        }

        private static string GenerateUid()
        {
            var guid = Guid.NewGuid();

            var octets = guid.ToByteArray();
            var littleEndianOrder = new byte[]
            { octets[15], octets[14], octets[13], octets[12], octets[11], octets[10], octets[9], octets[8],
                    octets[6], octets[7], octets[4], octets[5], octets[0], octets[1], octets[2], octets[3], 0 };

            return NormalizeUid("2.25." + new BigInteger(littleEndianOrder).ToString(CultureInfo.InvariantCulture));
        }

        public static string NormalizeUid(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
                return uid;

            var parts = uid.Split('.');
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (part.Length <= 1 || !part.All(static c => c is >= '0' and <= '9'))
                    continue;

                part = part.TrimStart('0');
                parts[i] = part.Length == 0 ? "0" : part;
            }

            return string.Join(".", parts);
        }
    }
}
