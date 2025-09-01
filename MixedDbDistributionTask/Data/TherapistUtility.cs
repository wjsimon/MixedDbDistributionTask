using System.Data.Common;

namespace MixedDbDistributionTask.Shared.Data
{
    public readonly record struct TherapistUtility(string Id, string Name)
    {
        public static TherapistDto From(DbDataReader dbReader)
        {
            return new TherapistDto() {
                Id = dbReader.GetString(0),
                Name = dbReader.GetString(1)
            };
        }

        public readonly record struct DbStub(string Id, string Name);
    }
}