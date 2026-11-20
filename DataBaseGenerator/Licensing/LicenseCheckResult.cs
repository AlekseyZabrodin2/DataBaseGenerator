using System;

namespace DataBaseGenerator.UI.Wpf.Licensing
{
    public sealed class LicenseCheckResult
    {
        public LicenseState State { get; init; }

        public int DaysLeft { get; init; }

        public DateTime? ExpiryDateUtc { get; init; }

        public bool IsValid => State is LicenseState.Trial or LicenseState.Licensed;
    }
}
