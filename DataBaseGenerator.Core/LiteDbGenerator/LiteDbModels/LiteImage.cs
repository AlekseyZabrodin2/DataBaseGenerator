using System;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels
{
    public sealed class LiteImage
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string SopInstanceUid { get; set; } = default!;

        public string SeriesInstanceUid { get; set; } = default!;
        public string StudyInstanceUid { get; set; } = default!;

        public string BodyPart { get; set; } = string.Empty; // Область инстанса
        public string Projection { get; set; } = string.Empty; // Проекция инстанса
        public string Laterality { get; set; } = string.Empty; // Латеральность снимка
        public double InstanceDose { get; set; } // Доза за снимок

        public DateTime AcquisitionTime { get; set; }
    }
}
