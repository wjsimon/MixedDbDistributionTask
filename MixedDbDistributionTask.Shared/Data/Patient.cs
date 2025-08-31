namespace MixedDbDistributionTask.Shared.Data
{
    public record struct Patient(string KvNummer, Practice Practice, string Name, int Age);
}