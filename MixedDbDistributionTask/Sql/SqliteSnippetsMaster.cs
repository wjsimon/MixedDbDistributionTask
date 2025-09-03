using Google.Protobuf.Collections;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MixedDbDistributionTask.Sql
{
    public static class SqliteSnippetsMaster
    {
        public const string Create = @"
            CREATE TABLE IF NOT EXISTS Practice(
                ik TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                company TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Patient(
                kv_nummer TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                age INTEGER
            );
            CREATE TABLE IF NOT EXISTS Remedy(
                diagnosis TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                is_fixed_type INTEGER
            );
            CREATE TABLE IF NOT EXISTS PatientsPracticeLink(
                patient_kv TEXT NOT NULL,
                practice_ik TEXT NOT NULL
            );";

        public const string InsertPractice = @"
            INSERT INTO Practice (ik, name, company)
            VALUES (@ik, @name, @company);";

        public const string InsertRemedy = @"
            INSERT INTO Remedy (diagnosis, name, is_fixed_type)
            VALUES (@diagnosis, @name, @is_fixed_type);";

        public const string InsertPatient = @"
            INSERT INTO Patient (kv_nummer, name, age)
            VALUES (@kv_nummer, @name, @age)";

        public const string InsertPatientPracticeRelation = @"
            INSERT INTO PatientsPracticeLink (patient_kv, practice_ik)
            VALUES (@patient_kv, @practice_ik)";

        public const string SelectPractice = @"SELECT * FROM Practice WHERE ik = @ik;";
        public const string SelectPractices = @"SELECT * FROM Practice;";

        public const string SelectPatientsForPractice = @"
            SELECT * FROM Patient
            JOIN PatientsPracticeLink ON Patient.kv_nummer = PatientsPracticeLink.patient_kv
            WHERE PatientsPracticeLink.practice_ik = @practice_ik;";

        public const string SelectRemedy = @"SELECT * FROM Remedy WHERE diagnosis = @diagnosis;";
        public const string SelectRemedies = @"SELECT * FROM Remedy";
        public const string SelectFixedRemedies = @"SELECT * FROM Remedy WHERE is_fixed_type = 1";
        public const string SelectPatient = @"SELECT * FROM Patient WHERE kv_nummer = @kv_nummer";
        //public const string SelectPatientsForPractice = @"SELECT * FROM Patient, Practice WHERE Patient.practice_ik = Practice.ik AND Practice.ik = @practice_ik";
    }
}