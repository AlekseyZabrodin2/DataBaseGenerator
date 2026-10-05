using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
{
    public sealed class OrderIdPatientWlRule : IGeneratorRule<int>
    {
        public IReadOnlyList<int> PatientIds { get; }
        private readonly Random _random = new();

        public OrderIdPatientWlRule(IReadOnlyList<int> patientIds)
        {
            PatientIds = patientIds;
        }

        public int Generate()
        {
            return PatientIds[_random.Next(PatientIds.Count)];
        }
    }
}
