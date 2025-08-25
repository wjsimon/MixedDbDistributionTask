namespace MixedDbDistributionTask.Sql
{
    public static class SqlSnippetsMaster
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
    }
}