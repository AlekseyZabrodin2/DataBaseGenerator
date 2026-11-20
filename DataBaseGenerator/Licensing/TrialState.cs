using System;

namespace DataBaseGenerator.UI.Wpf.Licensing
{
    public sealed class TrialState
    {
        public DateTime StartedAtUtc { get; set; }
        public DateTime LastSeenUtc { get; set; }
    }
}
