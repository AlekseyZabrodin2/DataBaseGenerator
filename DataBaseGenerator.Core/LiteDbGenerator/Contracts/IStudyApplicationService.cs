using System;
using System.Collections.Generic;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Contracts
{
    public interface IStudyApplicationService
    {
        PatientLiteDb UpsertPatient(PatientLiteDb patient);

        //Study AddStudy(Patient patient, Study study);

        //Study AddStudyWithSeriesAndImages(
        //    Patient patient,
        //    Study study,
        //    IEnumerable<(Series series, IEnumerable<Image> images)> seriesWithImages);

        IReadOnlyList<PatientLiteDb> SearchPatientsByLastName(string lastName, int limit = 100);
        IReadOnlyList<PatientLiteDb> SearchPatientsByFirstName(string firstName, int limit = 100);
        IReadOnlyList<PatientLiteDb> SearchPatientsByMiddleName(string middleName, int limit = 100);
        IReadOnlyList<PatientLiteDb> SearchPatientsByBirthDateRange(DateTime? from, DateTime? to, int limit = 100);
        IReadOnlyList<PatientLiteDb> SearchPatientsByAddress(string addressPart, int limit = 100);
        IReadOnlyList<PatientLiteDb> SearchPatientsByPhone(string phone, int limit = 100);
        IReadOnlyList<PatientLiteDb> SearchPatientsByName(string? firstName, string? lastName, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByDate(DateTime from, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByDateRange(DateTime? from, DateTime? to, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByUid(string studyInstanceUid, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByStudyId(string studyId, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByDescription(string descriptionPart, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByPatientLastName(string lastName, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByPatientFirstName(string firstName, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByPatientPatronymic(string patronymic, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByEffectiveDose(double minDose, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByModality(string modality, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByOperatorName(string operatorName, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesBySeriesDateRange(DateTime? from, DateTime? to, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByProjection(string projection, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByLaterality(string laterality, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByBodyPart(string bodyPart, int limit = 100);
        //IReadOnlyList<Study> SearchStudiesByImageAcquisitionDateRange(DateTime? from, DateTime? to, int limit = 100);

        //Series? FindSeriesBySeriesInstanceUid(string seriesInstanceUid);
        //IReadOnlyList<Series> SearchSeriesByStudy(string studyInstanceUid, int limit = 100);
        //IReadOnlyList<Series> SearchSeriesByModality(string modality, int limit = 100);
        //IReadOnlyList<Series> SearchSeriesByOperatorName(string operatorName, int limit = 100);
        //IReadOnlyList<Series> SearchSeriesByDateRange(DateTime? from, DateTime? to, int limit = 100);

        //Image? FindImageBySopInstanceUid(string sopInstanceUid);
        //IReadOnlyList<Image> SearchImagesByStudy(string studyInstanceUid, int limit = 100);
        //IReadOnlyList<Image> SearchImagesByBodyPart(string bodyPart, int limit = 100);
        //IReadOnlyList<Image> SearchImagesByProjection(string projection, int limit = 100);
        //IReadOnlyList<Image> SearchImagesByLaterality(string laterality, int limit = 100);
        //IReadOnlyList<Image> SearchImagesByAcquisitionDateRange(DateTime? from, DateTime? to, int limit = 100);

        //bool DeleteStudyWithRelations(string studyInstanceUid);
        bool DeletePatientWithRelations(string patientId);
    }
}
