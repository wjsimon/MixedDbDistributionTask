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
                practice_ik TEXT NOT NULL,
                name TEXT NOT NULL,
                age INTEGER,
                FOREIGN KEY(practice_ik) REFERENCES Practice(ik)
            );
            CREATE TABLE IF NOT EXISTS Remedy(
                diagnosis TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                is_fixed_type INTEGER
            );";

        public const string InsertPractice = @"
            INSERT INTO Practice (ik, name, company)
            VALUES (@ik, @name, @company);";

        public const string InsertRemedy = @"
            INSERT INTO Remedy (diagnosis, name, is_fixed_type)
            VALUES (@diagnosis, @name, @is_fixed_type);";

        public const string InsertPatient = @"
            INSERT INTO Patient (kv_nummer, practice_ik, name, age)
            VALUES (@kv_nummer, @practice_ik, @name, @age)";

        public const string SelectPractices = @"SELECT * From Practice;";
        public const string SelectRemedies = @"SELECT * From Remedy";
        public const string SelectFixedRemedies = @"SELECT * From Remedy WHERE is_fixed_type = 1";
        public const string SelectPatientsForPractice = @"SELECT * FROM Patient, Practice WHERE Patient.practice_ik = Practice.ik AND Practice.ik = @practice_ik";
    }
}