using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Shared.Data;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Services
{
    internal static class DatabaseReaderService
    {
        public static Practice[] GetPractices(DbIndex dbIndex)
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

        public static Remedy[] GetFixedRemedies(DbIndex dbIndex)
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

        public static Remedy[] GetTenantRemedies(DbIndex tenantIndex)
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

        public static Patient[] GetPatients(DbIndex master, string practiceIk)
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
                    using var practiceCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPractice, connection);
                    practiceCommand.Parameters.AddWithValue("@ik", sr.GetString(1));

                    using var practiceSr = practiceCommand.ExecuteReader();

                    if (!practiceSr.HasRows) { throw new InvalidDataException("invalid request, practice could not be queried"); }

                    Practice practice = default;
                    while (practiceSr.Read())
                    {
                        practice = new Practice(
                            sr.GetString(0),
                            sr.GetString(1),
                            sr.GetString(2));

                        break;
                    }

                    if (practice == default) { throw new InvalidDataException("invalid request, practice was found but not assigned"); }

                    patients.Add(
                        new Patient(
                            sr.GetString(0),
                            practice,
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

    }
}
