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
                id TEXT PRIMARY KEY,
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
            );
            CREATE TABLE IF NOT EXISTS TherapistsPracticesLink(
                therapist_id TEXT NOT NULL,
                practice_ik TEXT NOT NULL
            );";

        public const string InsertAppointment = @"
            INSERT INTO Appointment (id, starttime, endtime, therapist, patient, practice, remedy)
            VALUES (@id, @starttime, @endtime, @therapist, @patient, @practice, @remedy)
        ";

        public const string InsertTherapist = @"
            INSERT INTO Therapist (id, name)
            VALUES (@id, @name);
        ";

        public const string InserTherapistPracticeRelation = @"
            INSERT INTO TherapistsPracticesLink (therapist_id, practice_ik)
            VALUES (@therapist_id, @practice_ik)
        ";

        public const string SelectTherapist = @"SELECT * FROM Therapist WHERE id = @id;";
        public const string SelectRemedy = @"SELECT * FROM Remedy WHERE diagnosis = @diagnosis;"; //technically not needed, but used for clarity at calling point
        public const string SelectAppointmentsForPatientForPractice = @"SELECT * FROM Appointment WHERE patient = @patient_kv AND practice = @practice_ik;";
        public const string SelectAppointmentsForTherapist = @"SELECT * FROM Appointment WHERE therapist = @therapist_id";
    }
}