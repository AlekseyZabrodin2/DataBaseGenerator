using System;
using System.Collections.Generic;
using System.Linq;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Study
{
    public sealed class RandomBodyPartsRule : IGeneratorRule<string[]>
    {
        private static readonly Random _random = new Random();

        private static readonly string[] _bodyParts = new[]
        {
            "Clavicle",
            "Esophagus",
            "Chest",
            "Abdomen",
            "Skull",
            "Spine",
            "Pelvis",
            "Femur",
            "Shoulder",
            "Knee",
            "Ankle",
            "Wrist",
            "Elbow",
            "Hip",
            "Ribs",
            "Sternum",
            "Scapula",
            "Tibia",
            "Fibula",
            "Humerus"
        };

        public string[] Generate()
        {
            var count = _random.Next(1, 4);

            var shuffled = _bodyParts
                .OrderBy(x => _random.Next())
                .Take(count)
                .ToArray();

            return shuffled;
        }
    }
}
