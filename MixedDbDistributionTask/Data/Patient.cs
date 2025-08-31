namespace MixedDbDistributionTask.Data
{
    public record struct Patient(string KvNummer, string PracticeIk, string Name, int Age);
}