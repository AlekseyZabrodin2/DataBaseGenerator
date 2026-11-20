using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using DataBaseGenerator.Core.DicomGenerator.DicomGeneratorModels;
using DataBaseGenerator.Core.GeneratorRules.Patient;
using DataBaseGenerator.Core.GeneratorRules.WorkList;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.OrthancWorklists;

public static class OrthancWorklistFactory
{
    /// <summary>Same prefix as the MySQL patient generator.</summary>
    public const string PatientIdPrefix = "MXE";

    private const int PatientIdDigits = 7;
    private const int PersonNameComponentMax = 64;
    private const int AddressMax = 1024;
    private const int CommentsMax = 10240;
    private static int _batchSeed;

    /// <summary>
    /// Builds one worklist JSON body per patient, using the same patient rules as the MySQL generator.
    /// Patient IDs are numbered from <paramref name="firstPatientNumber"/>.
    /// </summary>
    public static IReadOnlyList<string> Create(
        int count,
        string modality,
        string stationAeTitle,
        long firstPatientNumber,
        Action<PatientGeneratorDto>? applyOptions = null)
    {
        if (count <= 0)
            return Array.Empty<string>();

        var modalityCode = string.IsNullOrWhiteSpace(modality) ? "DX" : modality.Trim();
        var aeTitle = string.IsNullOrWhiteSpace(stationAeTitle) ? "MODALITY" : stationAeTitle.Trim();
        var batch = Interlocked.Increment(ref _batchSeed);
        var stamp = DateTime.Now.ToString("yyMMddHHmm", CultureInfo.InvariantCulture);

        var generator = CreateGenerator();
        applyOptions?.Invoke(generator);

        var bodies = new List<string>(count);
        for (var index = 0; index < count; index++)
        {
            var start = DateTime.Now.AddMinutes(index);
            var patientId = PatientIdPrefix
                + (firstPatientNumber + index).ToString(new string('0', PatientIdDigits), CultureInfo.InvariantCulture);

            var lastName = generator.LastName.GenerateLastName(generator);
            var firstName = generator.FirstName.GenerateFirstName(generator);
            var middleName = generator.MiddleName.GenerateMiddleNames(generator);
            var birthDate = generator.BirthDate.GenerateBirthdate(generator);
            var sex = generator.Sex.Generate();
            var address = generator.Address.Generate();
            var addInfo = generator.AddInfo.Generate();
            var accession = $"{stamp}{(batch % 256):X2}{index:X4}";
            var description = new RandomStudyDescriptionRule().Generate();
            var step = ScheduledStep(modalityCode, aeTitle, start, description, accession);
            var tags = PatientTags(
                patientId,
                lastName,
                firstName,
                middleName,
                sex,
                address,
                telephone: string.Empty,
                addInfo,
                accession,
                description,
                step);

            if (birthDate.HasValue)
                tags["PatientBirthDate"] = birthDate.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            bodies.Add(JsonSerializer.Serialize(new Dictionary<string, object> { ["Tags"] = tags }));
        }

        return bodies;
    }

    /// <summary>
    /// One worklist from the values typed on the page.
    /// An empty description is filled from <see cref="RandomStudyDescriptionRule"/>, the same list as MySQL.
    /// </summary>
    public static string CreateOne(
        OrthancWorklistPatientInput patient,
        string modality,
        string stationAeTitle)
    {
        if (patient == null)
            throw new ArgumentNullException(nameof(patient));
        if (string.IsNullOrWhiteSpace(patient.PatientId))
            throw new ArgumentException("Patient ID is required.", nameof(patient));

        var modalityCode = string.IsNullOrWhiteSpace(modality) ? "DX" : modality.Trim();
        var aeTitle = string.IsNullOrWhiteSpace(stationAeTitle) ? "MODALITY" : stationAeTitle.Trim();
        var description = string.IsNullOrWhiteSpace(patient.Description)
            ? new RandomStudyDescriptionRule().Generate()
            : patient.Description;
        var accession = DateTime.Now.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture);
        var step = ScheduledStep(modalityCode, aeTitle, DateTime.Now, description, accession);
        var tags = PatientTags(
            patient.PatientId.Trim(),
            patient.LastName,
            patient.FirstName,
            patient.MiddleName,
            patient.Sex,
            patient.Address,
            patient.Telephone,
            patient.Comments,
            accession,
            description,
            step);

        if (patient.BirthDate.HasValue)
            tags["PatientBirthDate"] = patient.BirthDate.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        return JsonSerializer.Serialize(new Dictionary<string, object> { ["Tags"] = tags });
    }

    private static Dictionary<string, object> PatientTags(
        string patientId,
        string lastName,
        string firstName,
        string middleName,
        string sex,
        string address,
        string telephone,
        string comments,
        string accession,
        string description,
        Dictionary<string, object> step)
    {
        var procedureId = Component(accession, 16);
        var tags = new Dictionary<string, object>
        {
            ["SpecificCharacterSet"] = "ISO_IR 192",
            ["PatientID"] = patientId,
            ["PatientName"] = $"{Component(lastName, PersonNameComponentMax)}"
                + $"^{Component(firstName, PersonNameComponentMax)}"
                + $"^{Component(middleName, PersonNameComponentMax)}",
            ["PatientSex"] = Component(sex, 16),
            ["PatientAddress"] = Component(address, AddressMax),
            ["PatientComments"] = Component(comments, CommentsMax),
            ["AccessionNumber"] = procedureId,
            ["RequestedProcedureID"] = procedureId,
            ["RequestedProcedureDescription"] = Component(description, 64),
            ["ReferringPhysicianName"] = Component(new RandomReferringPhysiciansNameRule().Generate(), PersonNameComponentMax),
            ["StudyInstanceUID"] = UidGenerator.GenerateStudyUid(),
            ["ScheduledProcedureStepSequence"] = new[] { step }
        };

        var phone = Component(telephone, 16);
        if (phone.Length > 0)
            tags["PatientTelephoneNumbers"] = phone;

        return tags;
    }

    private static Dictionary<string, object> ScheduledStep(
        string modality,
        string aeTitle,
        DateTime start,
        string description,
        string procedureId) => new()
    {
        ["Modality"] = modality,
        ["ScheduledStationAETitle"] = aeTitle,
        ["ScheduledProcedureStepStartDate"] = start.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
        ["ScheduledProcedureStepStartTime"] = start.ToString("HHmmss", CultureInfo.InvariantCulture),
        ["ScheduledProcedureStepDescription"] = Component(description, 64),
        ["ScheduledProcedureStepID"] = Component(procedureId, 16),
        ["ScheduledProcedureStepStatus"] = "SCHEDULED"
    };

    private static PatientGeneratorDto CreateGenerator() => new(
        new OrderIdPatientRule(),
        new RandomLastNameRule(),
        new RandomFirstNameRule(),
        new RandomMiddleNameRule(),
        new OrderPatientIdRule(),
        new RandomBirthDateRule(new DateTime()),
        new RandomSexRule(),
        new RandomAddressRule(),
        new RandomPhoneRule(),
        new RandomAddInfoRule(),
        new RandomOccupationRule());

    /// <summary>Trims a value to a DICOM length limit and removes the PN component separator.</summary>
    private static string Component(string value, int max)
    {
        var text = (value ?? string.Empty).Replace('^', ' ').Trim();
        return text.Length <= max ? text : text.Substring(0, max).TrimEnd();
    }
}