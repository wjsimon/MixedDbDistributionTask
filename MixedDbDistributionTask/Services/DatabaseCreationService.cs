using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Classes;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Shared.Data;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Services
{
    internal class DatabaseCreationService
    {
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

        public bool GenerateMasterDebugData(DbIndex dbIndex)
        {
            var practices = new PracticeUtility.DbStub[]
            {
                new PracticeUtility.DbStub("practice1", "Practice #1",  "The Practice Company"),
                new PracticeUtility.DbStub("practice2", "Leaf and Machine", "The Practice Company"),
                new PracticeUtility.DbStub("practice3", "Not a Practice", "Some Competition")
            };

            //debug insertions for population
            DatabaseWriter.InsertPractices(dbIndex, practices);

            var fixedRemedies = new RemedyUtility.DbStub[]
            {
                new RemedyUtility.DbStub("bad", "The Bad One", 1),
                new RemedyUtility.DbStub("evenworse", "Wouldn't want to be you", 1),
                new RemedyUtility.DbStub("good", "All good buddy", 1)
            };

            DatabaseWriter.InsertRemedies(dbIndex, fixedRemedies);

            var patients = new PatientUtility.DbStub[]
            {
                    new PatientUtility.DbStub("0", "practice1", "Wilhelm Simon", 29),
                    new PatientUtility.DbStub("1", "practice1", "Hannes Roever", -1),
                    new PatientUtility.DbStub("2", "practice2", "Raphael Schweda", -1)
            };

            DatabaseWriter.InsertPatients(dbIndex, patients);
            return true;
        }

        public bool GenerateTenantDebugData(DbIndex dbIndex)
        {
            var therapists = new TherapistUtility.DbStub[]
            {
                new TherapistUtility.DbStub("therapist1", "Viktor Frankenstein")
            };

            DatabaseWriter.InsertTherapists(dbIndex, therapists);

            var appointments = new AppointmentUtility.DbStub[]
            {
                    new AppointmentUtility.DbStub(
                        "appointment1",
                        "2025-08-31 22:30:00",
                        "2025-08-31 22:31:00",
                        "therapist1",
                        "0",
                        "practice1",
                        "evenworse"
                    )
            };

            DatabaseWriter.InsertAppointments(dbIndex, appointments);
            return true;
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

        private string MakeSqliteDataSource(string path, string name)
            => $"Data Source={MakeSqliteFileLocation(path, name)}";

        private string MakeSqliteFileLocation(string path, string name)
            => $"{path}\\{name}.db";
    }
}