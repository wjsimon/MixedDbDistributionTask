namespace MixedDbDistributionTask.Shared.Data
{
    public record struct Appointment(
        string Id,
        DateTime StartTime,
        DateTime Endtime,
        Therapist Therapist,
        Patient Patient,
        Practice Practice,
        Remedy Remedy);
}