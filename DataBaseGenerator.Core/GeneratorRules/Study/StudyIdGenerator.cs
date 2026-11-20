using System;

namespace DataBaseGenerator.Core.GeneratorRules.Study
{
    public static class StudyIdGenerator
    {

        public static string GenerateStudyId()
        {
            var studyId = $"STD-{DateTime.Now.ToString("yyMMddHHmmss")}";

            return studyId;
        }
    }
}
