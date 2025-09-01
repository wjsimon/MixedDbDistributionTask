using System.Data.Common;

namespace MixedDbDistributionTask.Shared.Data
{
    public readonly record struct Remedy(string Diagnosis, string Name, bool IsFixed)
    {
        public static Remedy From(DbDataReader dbReader)
        {
            return new Remedy(
                dbReader.GetString(0),
                dbReader.GetString(1),
                dbReader.GetInt32(2) > 0);
        }
    }
}