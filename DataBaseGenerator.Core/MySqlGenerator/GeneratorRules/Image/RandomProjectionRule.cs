using System;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Image
{
    public class RandomProjectionRule : IGeneratorRule<string>
    {
        private static readonly Random _random = new Random();

        private static readonly string[] _projections =
        {
            "Antero-Posterior",
            "Postero-Anterior",
            "Lateral",
            "Right Lateral",
            "Left Lateral",
            "Axial",
            "Submentovertical",
            "Caldwell",
            "Waters",
            "Schüller",
            "Law",
            "Towne",
            "Ferguson",
            "Oblique Cervical",
            "Oblique Lumbar",
            "Lordotic",
            "Decubitus",
            "Lateral Decubitus",
            "Tangential",
            "Sunrise",
            "Merchant",
            "Rosenberg",
            "Not Applicable",
            "Unknown"
        };

        public string Generate()
        {
            var laterality = _projections[_random.Next(0, _projections.Length)];

            return laterality;
        }
    }
}
