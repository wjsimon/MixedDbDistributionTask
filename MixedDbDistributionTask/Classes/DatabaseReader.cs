using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Shared.Data;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Classes
{
    internal static class DatabaseReader
    {
        public static PracticeDto[] GetPractices(DbIndex dbIndex)
        {
            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPractices, connection);
            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<PracticeDto> practices = [];
                while (sr.Read())
                {
                    practices.Add(PracticeUtility.DTO(sr));
                }

                return practices.ToArray();
            }
            else
            {
                return [];
            }
        }

        public static RemedyDto[] GetFixedRemedies(DbIndex dbIndex)
        {
            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectFixedRemedies, connection);
            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<RemedyDto> remedies = [];
                while (sr.Read())
                {
                    remedies.Add(RemedyUtility.DTO(sr));
                }

                return remedies.ToArray();
            }
            else
            {
                return [];
            }
        }

        public static RemedyDto[] GetTenantRemedies(DbIndex tenantIndex)
        {
            using var connection = new SqliteConnection(tenantIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectRemedies, connection);
            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<RemedyDto> remedies = [];
                while (sr.Read())
                {
                    remedies.Add(RemedyUtility.DTO(sr));
                }

                return remedies.ToArray();
            }
            else
            {
                return [];
            }
        }

        public static PatientDto[] GetPatientsForPractice(DbIndex dbIndex, string practiceIk)
        {
            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPatientsForPractice, connection);
            sqlCommand.Parameters.AddWithValue("@practice_ik", practiceIk);

            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<PatientDto> patients = [];
                while (sr.Read())
                {
                    patients.Add(PatientUtility.DTO(sr, []));
                }

                return patients.ToArray();
            }
            else
            {
                return [];
            }
        }

        public static AppointmentDto[] GetAppointmentsForPatientForPractice(DbIndex masterDb, DbIndex tenantDb, string patientKv, string practiceIk)
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
                List<AppointmentDto> appointments = new List<AppointmentDto>();

                while (sr.Read())
                {
                    appointments.Add(AppointmentUtility.DTO(
                        sr,
                        GetSingleTherapist(connectionTenant, sr.GetString(3)),
                        GetSinglePatient(connectionMaster, sr.GetString(4)),
                        GetSinglePractice(connectionMaster, sr.GetString(5)),
                        SearchSingleRemedy(connectionMaster, connectionTenant, sr.GetString(6)))
                    );
                }

                return appointments.ToArray();
            }
            else
            {
                return [];
            }
        }

        public static TherapistDto[] GetTherapists(DbIndex tenantDb)
        {
            using var connection = new SqliteConnection(tenantDb.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(SqliteSnippetsTenant.SelectTherapists, connection);
            using var sr = sqlCommand.ExecuteReader();

            if (sr.HasRows)
            {
                List<TherapistDto> therapists = new List<TherapistDto>();

                while (sr.Read())
                {
                    therapists.Add(
                        TherapistUtility.DTO(sr));
                }

                return therapists.ToArray();
            }
            else
            {
                return [];
            }
        }

        public static AppointmentDto[] GetAppointmentsForTherapist(DbIndex masterDb, DbIndex tenantDb, string therapistId)
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
                List<AppointmentDto> appointments = new List<AppointmentDto>();

                while (sr.Read())
                {
                    appointments.Add(AppointmentUtility.DTO(
                        sr,
                        GetSingleTherapist(connectionTenant, sr.GetString(3)),
                        GetSinglePatient(connectionMaster, sr.GetString(4)),
                        GetSinglePractice(connectionMaster, sr.GetString(5)),
                        SearchSingleRemedy(connectionMaster, connectionTenant, sr.GetString(6)))
                    );
                }

                return appointments.ToArray();
            }
            else
            {
                return [];
            }
        }

        private static PracticeDto[] GetPracticesForPatient(SqliteConnection connection, string patientKv)
        {
            return [];

            //using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPractice, connection);
            ////sqlCommand.Parameters.AddWithValue("@ik", ik);

            //using var sr = sqlCommand.ExecuteReader();

            //if (!sr.HasRows) { throw new InvalidDataException("invalid request, query returned no rows"); }

            //PracticeDto? practice = null;
            //while (sr.Read())
            //{
            //    practice = PracticeUtility.DTO(sr);
            //    break;
            //}

            //if (practice == null) { throw new InvalidDataException("invalid request, practice was found but not assigned"); }

            //return practice;

        }

        private static PracticeDto GetSinglePractice(SqliteConnection connection, string ik)
        {
            using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPractice, connection);
            sqlCommand.Parameters.AddWithValue("@ik", ik);

            using var sr = sqlCommand.ExecuteReader();

            if (!sr.HasRows) { throw new InvalidDataException("invalid request, query returned no rows"); }

            PracticeDto? practice = null;
            while (sr.Read())
            {
                practice = PracticeUtility.DTO(sr);
                break;
            }

            if (practice == null) { throw new InvalidDataException("invalid request, practice was found but not assigned"); }

            return practice;
        }

        private static TherapistDto GetSingleTherapist(SqliteConnection connection, string id)
        {
            using var sqlCommand = new SqliteCommand(SqliteSnippetsTenant.SelectTherapist, connection);
            sqlCommand.Parameters.AddWithValue("@id", id);

            using var sr = sqlCommand.ExecuteReader();

            if (!sr.HasRows) { throw new InvalidDataException("invalid request, query returned no rows"); }

            TherapistDto? therapist = null;
            while (sr.Read())
            {
                therapist = TherapistUtility.DTO(sr);
                break;
            }

            if (therapist == null) { throw new InvalidDataException("invalid request, therapist was found but not assigned"); }

            return therapist;
        }

        private static PatientDto GetSinglePatient(SqliteConnection connection, string kv)
        {
            return null;
            //using var sqlCommand = new SqliteCommand(SqliteSnippetsMaster.SelectPatient, connection);
            //sqlCommand.Parameters.AddWithValue("@kv_nummer", kv);

            //using var sr = sqlCommand.ExecuteReader();

            //if (!sr.HasRows) { throw new InvalidDataException("invalid request, query returned no rows"); }

            //PatientDto? patient = null;
            //while (sr.Read())
            //{
            //    patient = PatientUtility.From(sr, GetSinglePractice(connection, sr.GetString(1))); //possible since they are both on the master
            //    break;
            //}

            //if (patient == null) { throw new InvalidDataException("invalid request, patient was found but not assigned"); }

            //return patient;
        }

        private static RemedyDto SearchSingleRemedy(SqliteConnection connectionMaster, SqliteConnection connectionTenant, string diagnosis)
        {
            //need to search for the remedy here! it could be in either db, but has to be in one of them;
            var remedy = TryGetSingleRemedy(connectionMaster, SqliteSnippetsMaster.SelectRemedy, diagnosis);
            if (remedy == null)
            {
                remedy = TryGetSingleRemedy(connectionTenant, SqliteSnippetsTenant.SelectRemedy, diagnosis);

                if (remedy == null) { throw new InvalidDataException($"requested diagnosis unknown"); }
            }

            return remedy;
        }

        private static RemedyDto? TryGetSingleRemedy(SqliteConnection connection, string snippet, string diagnosis)
        {
            using var sqlCommand = new SqliteCommand(snippet, connection);
            sqlCommand.Parameters.AddWithValue("@diagnosis", diagnosis);

            using var sr = sqlCommand.ExecuteReader();

            if (!sr.HasRows) { return null; } //valid

            RemedyDto? remedy = null;
            while (sr.Read())
            {
                remedy = RemedyUtility.DTO(sr);
                break;
            }

            if (remedy == null) { throw new InvalidDataException("invalid request, therapist was found but not assigned"); }

            return remedy;
        }
    }
}
