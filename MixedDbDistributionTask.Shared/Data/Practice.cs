using System.Data.Common;

namespace MixedDbDistributionTask.Shared.Data
{
    public readonly record struct Practice(string Ik, string Name, string Company)
    {
        public static Practice From(DbDataReader dbReader)
        {
            return new Practice(
                dbReader.GetString(0),
                dbReader.GetString(1),
                dbReader.GetString(2));
        }
    }
}
