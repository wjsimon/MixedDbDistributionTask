using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Shared.Data;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Classes
{
    internal static class DatabaseWriter
    {
        public static int InsertPractices(DbIndex dbIndex, params PracticeUtility.DbStub[]? practices)
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

        public static int InsertRemedies(DbIndex dbIndex, params RemedyUtility.DbStub[]? remedies)
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

        public static int InsertPatients(DbIndex dbIndex, params PatientUtility.DbStub[]? patients)
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

        public static int InsertTherapists(DbIndex dbIndex, params TherapistUtility.DbStub[]? therapists)
        {
            if (therapists == null || therapists.Length == 0) { return 0; }

            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            int inserted = 0;
            using (var transaction = connection.BeginTransaction())
            {
                var sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = SqliteSnippetsTenant.InsertTherapist;

                var idParam = sqlCommand.CreateParameter();
                idParam.ParameterName = "@id";

                var nameParam = sqlCommand.CreateParameter();
                nameParam.ParameterName = "@name";


                sqlCommand.Parameters.AddRange([idParam, nameParam]);

                for (int i = 0; i < therapists.Length; i++)
                {
                    idParam.Value = therapists[i].Id;
                    nameParam.Value = therapists[i].Name;

                    inserted += sqlCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            return inserted;
        }

        public static int InsertAppointments(DbIndex dbIndex, params AppointmentUtility.DbStub[]? appointments)
        {
            if (appointments == null || appointments.Length == 0) { return 0; }

            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            int inserted = 0;
            using (var transaction = connection.BeginTransaction())
            {
                var sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = SqliteSnippetsTenant.InsertAppointment;

                var idParam = sqlCommand.CreateParameter();
                idParam.ParameterName = "@id";

                var startParam = sqlCommand.CreateParameter();
                startParam.ParameterName = "@starttime";

                var endParam = sqlCommand.CreateParameter();
                endParam.ParameterName = "@endtime";

                var therapistParam = sqlCommand.CreateParameter();
                therapistParam.ParameterName = "@therapist";

                var patientParam = sqlCommand.CreateParameter();
                patientParam.ParameterName = "@patient";

                var practiceParam = sqlCommand.CreateParameter();
                practiceParam.ParameterName = "@practice";

                var remedyParam = sqlCommand.CreateParameter();
                remedyParam.ParameterName = "@remedy";

                sqlCommand.Parameters.AddRange([idParam, startParam, endParam, therapistParam, patientParam, practiceParam, remedyParam]);

                for (int i = 0; i < appointments.Length; i++)
                {
                    idParam.Value = appointments[i].Id;
                    startParam.Value = appointments[i].StartTime;
                    endParam.Value = appointments[i].EndTime;
                    therapistParam.Value = appointments[i].TherapistId;
                    patientParam.Value = appointments[i].PatientKv;
                    practiceParam.Value = appointments[i].PracticeIk;
                    remedyParam.Value = appointments[i].RemedyDiagnosis;

                    inserted += sqlCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            return inserted;
        }

        public static bool InsertSinglePractice(DbIndex dbIndex, PracticeUtility.DbStub practice)
            => InsertSinglePractice(dbIndex, practice.Ik, practice.Name, practice.Company);

        public static bool InsertSinglePractice(DbIndex dbIndex, string ik, string name, string company)
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
    }
}
