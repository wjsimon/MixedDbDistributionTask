using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Services
{
    public static class DatabaseWriterService
    {
        public static int InsertPractices(DbIndex dbIndex, params PracticeDto[]? practices)
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

        public static int InsertRemedies(DbIndex dbIndex, params RemedyDto[]? remedies)
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

        public static int InsertPatients(DbIndex dbIndex, params PatientDto[]? patients)
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

        public static bool InsertSinglePractice(DbIndex dbIndex, Practice practice)
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
