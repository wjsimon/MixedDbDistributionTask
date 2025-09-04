using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Classes;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Shared.Data;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Services
{
    //since this is for administration purposes only, no allowance checks are made; this is available globally
    internal class DatabaseCreationService
    {
        public DatabaseCreationService(IConfiguration configuration)
        {
            _configuration = configuration;

            CheckForDatabases();
        }

        private readonly IConfiguration _configuration;
        private Dictionary<string, DbIndex> _availableDatabases = [];

        public DbIndex MasterIndex => _availableDatabases["master"];
        public bool MasterAvailable => _availableDatabases.ContainsKey("master");

        public bool CreateMasterDbSafe(string location)
            => CreateSafe(location, "master", true);

        public bool CreateTenantDbSafe(string location, string tenantId)
            => CreateSafe(location, tenantId, false);

        public DbIndex GetIndex(string id) =>
            _availableDatabases[id];

        public string[] GetAvailableDatabases()
            => _availableDatabases.Keys.ToArray();

        public bool TryGetIndex(string id, out DbIndex index)
            => _availableDatabases.TryGetValue(id, out index);

        public bool TenantValid(string tenantId)
            => _availableDatabases.ContainsKey(tenantId);

        public void GenerateMasterDebugData(DbIndex master)
        {
            var practices = new PracticeUtility.DbStub[]
            {
                new PracticeUtility.DbStub("practice1", "Practice #1",  "The Practice Company"),
                new PracticeUtility.DbStub("practice2", "Leaf and Machine", "The Practice Company"),
                new PracticeUtility.DbStub("practice3", "Not a Practice", "Some Competition"),
                new PracticeUtility.DbStub("practice4", "The Fun House", "We Have Very Good Lawyers Corp."),
                new PracticeUtility.DbStub("practice5", "No Worries Therapies", "We Have Very Good Lawyers Corp.")
            };

            //debug insertions for population
            DatabaseWriter.InsertPractices(master, practices);

            var fixedRemedies = new RemedyUtility.DbStub[]
            {
                new RemedyUtility.DbStub("bad", "The Bad One", 1),
                new RemedyUtility.DbStub("evenworse", "Wouldn't want to be you", 1),
                new RemedyUtility.DbStub("good", "All good buddy", 1),
            };

            DatabaseWriter.InsertRemedies(master, fixedRemedies);

            var patients = new PatientUtility.DbStub[]
            {
                    new PatientUtility.DbStub("0", "practice1", "Wilhelm Simon", 29),
                    new PatientUtility.DbStub("1", "practice1", "Hannes Roever", -1),
                    new PatientUtility.DbStub("2", "practice2", "Raphael Schweda", -1)
            };

            DatabaseWriter.InsertPatients(master, patients);

            var relations = new PatientUtility.PracticeRelationStub[]
            {
                new PatientUtility.PracticeRelationStub("0", "practice1"),
                new PatientUtility.PracticeRelationStub("0", "practice2"),
                new PatientUtility.PracticeRelationStub("0", "practice4"),
                new PatientUtility.PracticeRelationStub("1", "practice1"),
                new PatientUtility.PracticeRelationStub("1", "practice4"),
                new PatientUtility.PracticeRelationStub("1", "practice1"),
                new PatientUtility.PracticeRelationStub("2", "practice2"),
                new PatientUtility.PracticeRelationStub("2", "practice3")
            };

            DatabaseWriter.InsertPatientPracticeRelations(master, relations);
        }

        public void GenerateTenantDebugData(DbIndex tenantDb, string prefix)
        {
            var remedies = new RemedyUtility.DbStub[]
            {
                new RemedyUtility.DbStub("tea", "Tea", 0),
                new RemedyUtility.DbStub("badatcardgames", "Exposure Therapy", 0),
            };

            DatabaseWriter.InsertRemedies(tenantDb, remedies);

            var therapists = new TherapistUtility.DbStub[]
            {
                new TherapistUtility.DbStub($"{prefix}_therapist1", "Viktor Frankenstein"),
                new TherapistUtility.DbStub($"{prefix}_therapist2", "Albert Hofmann"),
                new TherapistUtility.DbStub($"{prefix}_therapist3", "Dr. Doom")
            };

            DatabaseWriter.InsertTherapists(tenantDb, therapists);

            var relations = new TherapistUtility.PracticeRelationStub[]
            {
                new TherapistUtility.PracticeRelationStub($"{prefix}_therapist1", "practice1"),
                new TherapistUtility.PracticeRelationStub($"{prefix}_therapist1", "practice2"),
                new TherapistUtility.PracticeRelationStub($"{prefix}_therapist2", "practice1"),
                new TherapistUtility.PracticeRelationStub($"{prefix}_therapist2", "practice2"),
                new TherapistUtility.PracticeRelationStub($"{prefix}_therapist2", "practice3"),
                new TherapistUtility.PracticeRelationStub($"{prefix}_therapist2", "practice4"),
                new TherapistUtility.PracticeRelationStub($"{prefix}_therapist2", "practice5"),
                new TherapistUtility.PracticeRelationStub($"{prefix}_therapist3", "practice1"),
                new TherapistUtility.PracticeRelationStub($"{prefix}_therapist3", "practice4")
            };

            DatabaseWriter.InsertTherapistPracticeRelation(tenantDb, relations);

            var appointments = new AppointmentUtility.DbStub[]
            {
                    new AppointmentUtility.DbStub(
                        "appointment1",
                        "2025-08-31 22:30:00",
                        "2025-08-31 22:31:00",
                        $"{prefix}_therapist1",
                        "0",
                        "practice4",
                        "bad"
                    ),
                    new AppointmentUtility.DbStub(
                        "appointment2",
                        "2025-08-31 12:47:00",
                        "2025-08-31 18:00:00",
                        $"{prefix}_therapist2",
                        "0",
                        "practice3",
                        "badatcardgames"
                    ),new AppointmentUtility.DbStub(
                        "appointment3",
                        "2025-08-31 22:30:00",
                        "2025-08-31 22:31:00",
                        $"{prefix}_therapist2",
                        "1",
                        "practice1",
                        "tea"
                    ),new AppointmentUtility.DbStub(
                        "appointment4",
                        "2025-08-31 22:30:00",
                        "2025-08-31 22:31:00",
                        $"{prefix}_therapist3",
                        "2",
                        "practice4",
                        "good"
                    ),
            };

            DatabaseWriter.InsertAppointments(tenantDb, appointments);
        }

        public void GenerateApiKeys(DbIndex master, string[] tenants)
        {
            DatabaseWriter.InsertApiKeys(master, tenants);
        }

        private bool CreateSafe(string location, string name, bool isMaster)
        {
            if (File.Exists(MakeSqliteFileLocation(location, name)))
            {
                _availableDatabases.TryAdd(name, new DbIndex(MakeSqliteDataSource(location, name)));
                return false;
            }
            else
            {
                return Create(location, name, isMaster);
            }
        }

        private bool Create(string location, string name, bool isMaster)
        {
            DbIndex dbIndex = new DbIndex(MakeSqliteDataSource(location, name));
            using var connection = new SqliteConnection(dbIndex.Source);
            connection.Open();

            using var sqlCommand = new SqliteCommand(isMaster ? SqliteSnippetsMaster.Create : SqliteSnippetsTenant.Create, connection);
            sqlCommand.ExecuteNonQuery();

            _availableDatabases.TryAdd(name, dbIndex);
            return true;
        }

        private void CheckForDatabases()
        {
            var loc = _configuration["ConnectionStrings:SqliteDbPath"];
            if (loc == null) { return; } //throw instead?

            var paths = CollectDatabases(loc);

            foreach (var path in paths)
            {
                if (IsSqliteDb(path))
                {
                    var key = path.Split('\\').Last().Split('.').First();
                    _availableDatabases.Add(key, new DbIndex(MakeSqliteDataSource(path)));
                }
            }
        }

        private string[] CollectDatabases(string location)
        {
            var candidates = Directory.GetFiles(location, "*.db");
            return candidates;
        }

        private static bool IsSqliteDb(string pathToFile) //yes, you can spoof a false-positive here
        {
            bool result = false;
            if (File.Exists(pathToFile))
            {
                using (FileStream stream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] header = new byte[16];

                    for (int i = 0; i < 16; i++)
                    {
                        header[i] = (byte)stream.ReadByte();
                    }

                    result = System.Text.Encoding.UTF8.GetString(header).Contains("SQLite format 3");
                    stream.Close();
                }
            }

            return result;
        }

        private string MakeSqliteDataSource(string path)
            => $"Data Source={path}";

        private string MakeSqliteDataSource(string path, string name)
            => $"Data Source={MakeSqliteFileLocation(path, name)}";

        private string MakeSqliteFileLocation(string path, string name)
            => $"{path}\\{name}.db";
    }
}