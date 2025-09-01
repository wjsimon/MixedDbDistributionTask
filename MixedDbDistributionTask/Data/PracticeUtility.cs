using System.Data.Common;

namespace MixedDbDistributionTask.Shared.Data
{
    public static class PracticeUtility
    {
        public static PracticeDto DTO(DbDataReader dbReader)
        {
            return new PracticeDto()
            {
                Ik = dbReader.GetString(0),
                Name = dbReader.GetString(1),
                Company = dbReader.GetString(2)
            };
        }

        public readonly record struct DbStub(string Ik, string Name, string Company);
    }
}
