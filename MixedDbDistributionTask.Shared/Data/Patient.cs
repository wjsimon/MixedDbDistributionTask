using System.Data.Common;

namespace MixedDbDistributionTask.Shared.Data
{
    public readonly record struct Patient(string KvNummer, Practice Practice, string Name, int Age)
    {
        public static Patient From(DbDataReader dbReader, Practice practice) //practice needs to be known beforehand; can always be acquired at the caller since the reader can be
        {
            return new Patient(
                dbReader.GetString(0),
                practice,
                dbReader.GetString(2),
                dbReader.GetInt32(3)
            );
        }
    }
}