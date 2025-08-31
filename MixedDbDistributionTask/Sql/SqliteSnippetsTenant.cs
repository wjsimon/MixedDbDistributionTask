namespace MixedDbDistributionTask.Sql
{
    public static class SqliteSnippetsTenant
    {
        public const string Create = @"
            CREATE TABLE IF NOT EXISTS Therapist(
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Appointment(
                id INTEGER PRIMARY KEY,
                starttime TEXT NOT NULL,
                endtime TEXT NOT NULL,
                therapist TEXT NOT NULL,
                patient TEXT NOT NULL,
                practice TEXT NOT NULL,
                remedy TEXT NOT NULL,
                FOREIGN KEY(therapist) REFERENCES Therapist(id)            
            );
            CREATE TABLE IF NOT EXISTS Remedy(
                diagnosis TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                is_fixed_type INTEGER
            );";
    }
}
