namespace MixedDbDistributionTask.Shared.Data
{
    public readonly record struct Appointment(
        string Id,
        DateTime StartTime,
        DateTime EndTime,
        Therapist Therapist,
        Patient Patient,
        Practice Practice,
        Remedy Remedy);
}