using Microsoft.Data.Sqlite;
using MixedDbDistributionTask.Classes;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Sql;

namespace MixedDbDistributionTask.Services
{
    internal class DatabaseCreationService
    {
        private Dictionary<string, DbIndex> _availableDatabases = [];

        public DbIndex MasterIndex => _availableDatabases["master"];

        public bool CreateMasterDbSafe(string location)
            => CreateSafe(location, "master", true);

        public bool CreateTenantDbSafe(string location, string tenantId)
            => CreateSafe(location, tenantId, false);

        public DbIndex GetIndex(string id) =>
            _availableDatabases[id];

        public bool TryGetIndex(string id, out DbIndex index)
            => _availableDatabases.TryGetValue(id, out index);

        public bool WriteMasterDebugData(DbIndex dbIndex)
        {
            var practices = new PracticeDto[]
            {
                    new PracticeDto() { Ik = "practice1", Name = "Practice #1", Company = "The Practice Company" },
                    new PracticeDto() { Ik = "practice2", Name = "Leaf and Machine", Company = "The Practice Company" },
                    new PracticeDto() { Ik = "pratice3", Name = "Not a Practice", Company = "Some Competition" }
            };

            //debug insertions for population
            DatabaseWriter.InsertPractices(dbIndex, practices);

            var fixedRemedies = new RemedyDto[]
            {
                    new RemedyDto() { Diagnosis = "bad", Name = "The Bad One", IsFixed = true },
                    new RemedyDto() { Diagnosis = "evenworse", Name = "Wouldn't want to be you", IsFixed = true },
                    new RemedyDto() { Diagnosis = "good", Name = "All good buddy", IsFixed = true }
            };

            DatabaseWriter.InsertRemedies(dbIndex, fixedRemedies);

            var patients = new PatientDto[]
            {
                    new PatientDto() { KvNummer = "0", PracticeIk = "practice1", Name = "Wilhelm Simon", Age = 29 },
                    new PatientDto() { KvNummer = "1", PracticeIk = "practice1", Name = "Hannes Roever", Age = -1 },
                    new PatientDto() { KvNummer = "2", PracticeIk = "practice2", Name = "Raphael Schweda", Age = -1 }
            };

            DatabaseWriter.InsertPatients(dbIndex, patients);
            return true;
        }

        public bool WriteTenantDebugData(DbIndex dbIndex)
        {
            var therapists = new TherapistDto[]
            {
                new TherapistDto() { Id = "therapist1", Name = "Viktor Frankenstein" }
            };

            DatabaseWriter.InsertTherapists(dbIndex, therapists);

            var appointments = new AppointmentDto[]
            {
                    new AppointmentDto() {
                        Id = "appointment1",
                        StartTime = new DateTime(2025, 08, 31, 22, 30, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = new DateTime(2025, 08, 31, 22, 31, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                        PatientKv = "0",
                        PracticeIk = "practice1",
                        TherapistId = "therapist1",
                        RemedyDiagnosis = "evenworse"
                    }
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