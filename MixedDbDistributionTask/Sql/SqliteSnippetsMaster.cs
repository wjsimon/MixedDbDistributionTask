using Microsoft.Data.Sqlite;

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
            );";

        public const string InsertPractice = @"
            INSERT INTO Practice (ik, name, company)
            VALUES (@ik, @name, @company);";

        public const string InsertRemedy = @"
            INSERT INTO Remedy (diagnosis, name, is_fixed_type)
            VALUES (@diagnosis, @name, @is_fixed_type);";

        public const string SelectPractices = @"SELECT * From Practice;";
        public const string SelectRemedies = @"SELECT * From Remedy";
        public const string SelectFixedRemedies = @"SELECT * From Remedy WHERE is_fixed_type = 1";
    }
}