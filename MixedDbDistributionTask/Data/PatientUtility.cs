using System.Data.Common;

namespace MixedDbDistributionTask.Shared.Data
{
    public static class PatientUtility
    {
        public static PatientDto From(DbDataReader dbReader, PracticeDto practice) //practice needs to be known beforehand; can always be acquired at the caller since the reader can be
        {
            return new PatientDto() { 
                KvNummer = dbReader.GetString(0),
                Practice = practice,
                Name = dbReader.GetString(2),
                Age = dbReader.GetInt32(3)
            };
        }

        public readonly record struct DbStub(string KvNummer, string PracticeIk, string Name, int Age);
    }
}