namespace DataBaseGenerator.Core.OrthancWorklists;

public sealed class OrthancWorklistEntry
{
    public string Id { get; init; } = string.Empty;

    public string PatientId { get; init; } = string.Empty;

    public string PatientName { get; init; } = string.Empty;

    public string PatientSex { get; init; } = string.Empty;

    public string PatientAddress { get; init; } = string.Empty;

    public string PatientTelephone { get; init; } = string.Empty; 
    
    public string PatientComments { get; init; } = string.Empty;

    public string PatientBirthDate { get; init; } = string.Empty;

    public string AccessionNumber { get; init; } = string.Empty;

    public string RequestedProcedureId { get; init; } = string.Empty;

    public string RequestedProcedureDescription { get; init; } = string.Empty;

    public string ScheduledProcedureStepId { get; init; } = string.Empty;

    public string ReferringPhysicianName { get; init; } = string.Empty;

    public string Modality { get; init; } = string.Empty;

    public string StationAeTitle { get; init; } = string.Empty;

    public string ScheduledStart { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}
