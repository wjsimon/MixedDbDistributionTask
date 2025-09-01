using System.Data.Common;

namespace MixedDbDistributionTask.Shared.Data
{
    public readonly record struct Therapist(string Id, string Name)
    {
        public static Therapist From(DbDataReader dbReader)
        {
            return new Therapist(
                dbReader.GetString(0),
                dbReader.GetString(1));
        }
    }
}