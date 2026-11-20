using System;

namespace DataBaseGenerator.UI.Wpf.Licensing
{
    public sealed class LicensePayload
    {
        public string Product { get; init; } = "DataBaseGenerator";

        public string LicenseId { get; init; } = string.Empty;

        public string Customer { get; init; } = string.Empty;

        public DateTime IssuedAtUtc { get; init; }

        public DateTime? ExpiresAtUtc { get; init; }
    }
}
