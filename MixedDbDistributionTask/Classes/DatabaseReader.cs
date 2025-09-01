using Grpc.Net.Client.Balancer;
using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Shared.Data;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Classes
{
    internal static class DatabaseReader
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
                    practices.Add(Practice.From(sr));
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
                    patients.Add(
                        new Patient(
                            sr.GetString(0),
                            GetSinglePractice(connection, sr.GetString(1)),
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

        public static Appointment[] GetAppointmentsForPatientForPractice(DbIndex masterDb, DbIndex tenantDb, string patientKv, string practiceIk)
        {
            using var connectionMaster = new SqliteConnection(masterDb.Source);
            using var connectionTenant = new SqliteConnection(tenantDb.Source);
            connectionMaster.Open();
            connectionTenant.Open(); //second connection does not need to be opened unless first is resolved, but it kinda doesn't matter? should be optimized for prod

            using var sqlCommand = new SqliteCommand(SqliteSnippetsTenant.SelectAppointmentsForPatientForPractice, connectionTenant);
            sqlCommand.Parameters.AddWithValue("@patient_kv", patientKv);
            sqlCommand.Parameters.AddWithValue("@practice_ik", practiceIk);

            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<Appointment> appointments = new List<Appointment>();

                while (sr.Read())
                {
                    appointments.Add(new Appointment(
                        sr.GetString(0),
                        DateTime.ParseExact(sr.GetString(1), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                        DateTime.ParseExact(sr.GetString(2), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                        GetSingleTherapist(connectionTenant, sr.GetString(3)),
                        GetSinglePatient(connectionMaster, sr.GetString(4)),
                        GetSinglePractice(connectionMaster, sr.GetString(5)),
                        SearchSingleRemedy(connectionMaster, connectionTenant, sr.GetString(6))
                    ));
                }

                return appointments.ToArray();
            }
            else
            {
                return [];
            }
        }

        public static Appointment[] GetAppointmentsForTherapist(DbIndex masterDb, DbIndex tenantDb, string therapistId)
        {
            using var connectionMaster = new SqliteConnection(masterDb.Source);
            using var connectionTenant = new SqliteConnection(tenantDb.Source);
            connectionMaster.Open();
            connectionTenant.Open(); //same here

            using var sqlCommand = new SqliteCommand(SqliteSnippetsTenant.SelectAppointmentsForTherapist, connectionTenant);
            sqlCommand.Parameters.AddWithValue("@therapist_id", therapistId);

            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<Appointment> appointments = new List<Appointment>();

                while (sr.Read())
                {
                    appointments.Add(new Appointment(
                        sr.GetString(0),
                        DateTime.ParseExact(sr.GetString(1), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                        DateTime.ParseExact(sr.GetString(2), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                        GetSingleTherapist(connectionTenant, sr.GetString(3)),
                        GetSinglePatient(connectionMaster, sr.GetString(4)),
                        GetSinglePractice(connectionMaster, sr.GetString(5)),
                        SearchSingleRemedy(connectionMaster, connectionTenant, sr.GetString(6))
                    ));
                }

                return appointments.ToArray();
            }
            else
            {
                return [];
            }
        }

        private static Practice GetSinglePractice(SqliteConnection connection, string ik)
        {
            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPractice, connection);
            sqlCommand.Parameters.AddWithValue("@ik", ik);

            using var sr = sqlCommand.ExecuteReader();

            if (!sr.HasRows) { throw new InvalidDataException("invalid request, query returned no rows"); }

            Practice practice = default;
            while (sr.Read())
            {
                practice = Practice.From(sr);
                break;
            }

            if (practice == default) { throw new InvalidDataException("invalid request, practice was found but not assigned"); }

            return practice;
        }

        private static Therapist GetSingleTherapist(SqliteConnection connection, string id)
        {
            using var sqlCommand = new SqliteCommand(SqliteSnippetsTenant.SelectTherapist, connection);
            sqlCommand.Parameters.AddWithValue("@id", id);

            using var sr = sqlCommand.ExecuteReader();

            if (!sr.HasRows) { throw new InvalidDataException("invalid request, query returned no rows"); }

            Therapist therapist = default;
            while (sr.Read())
            {
                therapist = Therapist.From(sr);
                break;
            }

            if (therapist == default) { throw new InvalidDataException("invalid request, therapist was found but not assigned"); }

            return therapist;
        }

        private static Patient GetSinglePatient(SqliteConnection connection, string kv)
        {
            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPatient, connection);
            sqlCommand.Parameters.AddWithValue("@kv_nummer", kv);

            using var sr = sqlCommand.ExecuteReader();

            if (!sr.HasRows) { throw new InvalidDataException("invalid request, query returned no rows"); }

            Patient patient = default;
            while (sr.Read())
            {
                patient = Patient.From(sr, GetSinglePractice(connection, sr.GetString(1))); //possible since they are both on the master
                break;
            }

            if (patient == default) { throw new InvalidDataException("invalid request, patient was found but not assigned"); }

            return patient;
        }

        private static Remedy SearchSingleRemedy(SqliteConnection connectionMaster, SqliteConnection connectionTenant, string diagnosis)
        {
            //need to search for the remedy here! it could be in either db, but has to be in one of them;
            var remedy = TryGetSingleRemedy(connectionMaster, SqliteSnippetsMaster.SelectRemedy, diagnosis);
            if (remedy == null)
            {
                remedy = TryGetSingleRemedy(connectionTenant, SqliteSnippetsTenant.SelectRemedy, diagnosis);

                if (remedy == null) { throw new InvalidDataException($"requested diagnosis unknown"); }
            }

            return (Remedy)remedy!;
        }

        private static Remedy? TryGetSingleRemedy(SqliteConnection connection, string snippet, string diagnosis)
        {
            using var sqlCommand = new SqliteCommand(snippet, connection);
            sqlCommand.Parameters.AddWithValue("@diagnosis", diagnosis);

            using var sr = sqlCommand.ExecuteReader();

            if (!sr.HasRows) { return null; } //valid

            Remedy remedy = default;
            while (sr.Read())
            {
                remedy = Remedy.From(sr);
                break;
            }

            if (remedy == default) { throw new InvalidDataException("invalid request, therapist was found but not assigned"); }

            return remedy;
        }
    }
}
