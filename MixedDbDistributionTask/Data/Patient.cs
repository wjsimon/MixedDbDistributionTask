namespace MixedDbDistributionTask.Data
{
    public record struct Patient(string KvNummer, Practice Practice, string Name, int Age);
}