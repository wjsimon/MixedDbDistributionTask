using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Services
{
    public class DatabaseCreationService
    {
        public void CreateMasterDb(string location)
            => Create(location, "master", true);

        public void CreateTenantDb(string location, string tenantId)
            => Create(location, tenantId, false);

        public bool GenerateMasterDebugData() { return true; }
        public bool GenerateTenantDebugData() { return true; }

        private void Create(string location, string name, bool isMaster)
        {
            using var connection = new SqliteConnection(MakeSqliteDataSource(location, name));
            connection.Open();

            using var sqlCommand = new SqliteCommand(isMaster ? SqlSnippetsMaster.Create : SqlSnippetsTenant.Create, connection);
            sqlCommand.ExecuteNonQuery();
        }

        private string MakeSqliteDataSource(string path, string name)
            => $"Data Source={path}\\{name}.db";
    }
}