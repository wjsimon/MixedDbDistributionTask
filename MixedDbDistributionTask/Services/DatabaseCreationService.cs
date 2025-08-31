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

        public Remedy[] GetFixedRemedies(DbIndex dbIndex) 
        {
            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectFixedRemedies, connection);
            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<Remedy> remedies = [];
                while (sr.Read())
                {
                    remedies.Add(
                        new Remedy(
                            sr.GetString(0),
                            sr.GetString(1),
                            sr.GetBoolean(2))
                        );
                }

                return remedies.ToArray();
            }
            else
            {
                return [];
            }
        }

        public Remedy[] GetTenantRemedies(DbIndex tenantIndex)
        {
            using var connection = new SqliteConnection(tenantIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectRemedies, connection);
            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<Remedy> remedies = [];
                while (sr.Read())
                {
                    remedies.Add(
                        new Remedy(
                            sr.GetString(0),
                            sr.GetString(1),
                            sr.GetBoolean(2))
                        );
                }

                return remedies.ToArray();
            }
            else
            {
                return [];
            }
        }

        public Patient[] GetPatients(DbIndex master, string practiceIk)
        {
            using var connection = new SqliteConnection(master.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPatientsForPractice, connection);
            sqlCommand.Parameters.AddWithValue("@practice_ik", practiceIk);

            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<Patient> patients = [];
                while (sr.Read())
                {
                    patients.Add(
                        new Patient(
                            sr.GetString(0),
                            sr.GetString(1),
                            sr.GetString(2),
                            sr.GetInt32(3))
                        );
                }

                return patients.ToArray();
            }
            else
            {
                return [];
            }
        }

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

        public int InsertRemedies(DbIndex dbIndex, params Remedy[]? remedies)
        {
            if (remedies == null || remedies.Length == 0) { return 0; }

            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            int inserted = 0;
            using (var transaction = connection.BeginTransaction())
            {
                var sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = SqliteSnippetsMaster.InsertRemedy;

                var diagnosisParam = sqlCommand.CreateParameter();
                diagnosisParam.ParameterName = "@diagnosis";

                var nameParam = sqlCommand.CreateParameter();
                nameParam.ParameterName = "@name";

                var isFixedParam = sqlCommand.CreateParameter();
                isFixedParam.ParameterName = "@is_fixed_type";

                sqlCommand.Parameters.AddRange([diagnosisParam, nameParam, isFixedParam]);

                for (int i = 0; i < remedies.Length; i++)
                {
                    diagnosisParam.Value = remedies[i].Diagnosis;
                    nameParam.Value = remedies[i].Name;
                    isFixedParam.Value = remedies[i].IsFixed;

                    inserted += sqlCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            return inserted;
        }

        public int InsertPatients(DbIndex dbIndex, params Patient[]? patients)
        {
            if (patients == null || patients.Length == 0) { return 0; }

            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            int inserted = 0;
            using (var transaction = connection.BeginTransaction())
            {
                var sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = SqliteSnippetsMaster.InsertPatient;

                var kvParam = sqlCommand.CreateParameter();
                kvParam.ParameterName = "@kv_nummer";

                var ikParam = sqlCommand.CreateParameter();
                ikParam.ParameterName = "@practice_ik";

                var nameParam = sqlCommand.CreateParameter();
                nameParam.ParameterName = "@name";
                
                var ageParam = sqlCommand.CreateParameter();
                ageParam.ParameterName = "@age";

                sqlCommand.Parameters.AddRange([kvParam, ikParam, nameParam, ageParam]);

                for (int i = 0; i < patients.Length; i++)
                {
                    kvParam.Value = patients[i].KvNummer;
                    ikParam.Value = patients[i].PracticeIk;
                    nameParam.Value = patients[i].Name;
                    ageParam.Value = patients[i].Age;

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