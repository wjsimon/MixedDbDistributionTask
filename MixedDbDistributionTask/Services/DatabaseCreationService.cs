using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Services
{
    public class DatabaseCreationService
    {
        public Dictionary<string, DbIndex> AvailableDatabases = [];

        public DbIndex CreateMasterDbSafe(string location)
            => CreateSafe(location, "master", true);

        public DbIndex CreateTenantDbSafe(string location, string tenantId)
            => CreateSafe(location, tenantId, false);

        public DbIndex CreateMasterDb(string location)
            => Create(location, "master", true);

        public DbIndex CreateTenantDb(string location, string tenantId)
            => Create(location, tenantId, false);

        public bool GenerateMasterDebugData() { return true; }
        public bool GenerateTenantDebugData() { return true; }

        public Practice[] GetPractices(DbIndex dbIndex)
        {
            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPractices, connection);
            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<Practice> practices = [];
                while (sr.Read())
                {
                    practices.Add(
                        new Practice(
                            sr.GetString(0),
                            sr.GetString(1),
                            sr.GetString(2))
                        );
                }

                return practices.ToArray();
            }
            else
            {
                return [];
            }
        }

        //public Practice? GetPractice(string ik)
        //{

        //}

        public int InsertPractices(DbIndex dbIndex, params Practice[]? practices)
        {
            if (practices == null || practices.Length == 0) { return 0; }

            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            int inserted = 0;
            using (var transaction = connection.BeginTransaction())
            {
                var sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = SqliteSnippetsMaster.InsertPractice;

                var ikParam = sqlCommand.CreateParameter();
                ikParam.ParameterName = "@ik";

                var nameParam = sqlCommand.CreateParameter();
                nameParam.ParameterName = "@name";

                var companyParam = sqlCommand.CreateParameter();
                companyParam.ParameterName = "@company";

                sqlCommand.Parameters.AddRange([ikParam, nameParam, companyParam]);

                for (int i = 0; i < practices.Length; i++)
                {
                    ikParam.Value = practices[i].Ik;
                    nameParam.Value = practices[i].Name;
                    companyParam.Value = practices[i].Company;

                    inserted += sqlCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            return inserted;
        }

        public bool InsertSinglePractice(DbIndex dbIndex, Practice practice)
            => InsertSinglePractice(dbIndex, practice.Ik, practice.Name, practice.Company);

        public bool InsertSinglePractice(DbIndex dbIndex, string ik, string name, string company)
        {
            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.InsertPractice, connection);
            sqlCommand.Parameters.AddWithValue("@ik", ik);
            sqlCommand.Parameters.AddWithValue("@name", name);
            sqlCommand.Parameters.AddWithValue("@company", company);

            int inserted = sqlCommand.ExecuteNonQuery();
            return inserted > 0;
        }

        private DbIndex CreateSafe(string location, string name, bool isMaster)
        {
            var source = MakeSqliteDataSource(location, name);
            if (File.Exists(source))
            {
                return new DbIndex(source);
            }
            else { return Create(location, name, isMaster); }
        }

        private DbIndex Create(string location, string name, bool isMaster)
        {
            DbIndex dbIndex = new DbIndex(MakeSqliteDataSource(location, name));
            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(isMaster ? SqliteSnippetsMaster.Create : SqliteSnippetsTenant.Create, connection);
            sqlCommand.ExecuteNonQuery();

            AvailableDatabases.TryAdd(name, dbIndex);
            return dbIndex;
        }

        private string MakeSqliteDataSource(string path, string name)
            => $"Data Source={path}\\{name}.db";
    }
}