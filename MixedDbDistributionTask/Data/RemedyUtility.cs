using System.Data.Common;

namespace MixedDbDistributionTask.Shared.Data
{
    public static class RemedyUtility
    {
        public static RemedyDto DTO(DbDataReader dbReader)
        {
            return new RemedyDto() { 
                Diagnosis = dbReader.GetString(0),
                Name = dbReader.GetString(1),
                IsFixed = dbReader.GetInt32(2) > 0
            };
        }

        public readonly record struct DbStub(string Diagnosis, string Name, int IsFixed);
    }
}