using System.Data.Common;

namespace MixedDbDistributionTask.Shared.Data
{
    public static class PatientUtility
    {
        public static PatientDto DTO(DbDataReader dbReader, PracticeDto[] practices) //practice needs to be known beforehand; can always be acquired at the caller since the reader can be
        {
            var dto = new PatientDto() { 
                KvNummer = dbReader.GetString(0),
                Name = dbReader.GetString(1),
                Age = dbReader.GetInt32(2)
            };

            dto.Practices.AddRange(practices);
            return dto;
        }

        public readonly record struct DbStub(string KvNummer, string PracticeIk, string Name, int Age);
        public readonly record struct PracticeRelationStub(string PatientKv, string PracticeIk);
    }
}