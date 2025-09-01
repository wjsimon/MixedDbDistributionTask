using Grpc.Core;
using MixedDbDistributionTask.Classes;
using MixedDbDistributionTask.Data;

namespace MixedDbDistributionTask.Services
{
    internal class AccessorService : Accessor.AccessorBase
    {
        public AccessorService(
            ILogger<AccessorService> logger,
            DatabaseCreationService dbcs)
        {
            _logger = logger;
            _dbcs = dbcs;
        }

        private readonly ILogger<AccessorService> _logger;
        private readonly DatabaseCreationService _dbcs;

        public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingReply { Message = "Hello " + request.Payload });
        }

        public override Task<DatabasesReply> GetDatabaseAvailability(DatabasesRequest request, ServerCallContext context)
        {
            var reply = new DatabasesReply();
            reply.MasterAvailable = _dbcs.MasterIndex != default;
            reply.AvailableDatabases.AddRange(_dbcs.GetAvailableDatabases());

            return Task.FromResult(reply);
        }

        public override Task<PracticesReply> GetPractices(PracticesRequest request, ServerCallContext context)
        {
            var reply = new PracticesReply();

            if (_dbcs.TryGetIndex("master", out DbIndex index))
            {
                reply.Practices.AddRange(DatabaseReader.GetPractices(index));
            }

            return Task.FromResult(reply);
        }

        public override Task<RemedyReply> GetRemedies(RemedyRequest request, ServerCallContext context)
        {
            var reply = new RemedyReply();
            
            if (_dbcs.TryGetIndex("master", out DbIndex master))
            {
                var fixedRemedies = DatabaseReader.GetFixedRemedies(master);
                reply.Remedies.AddRange(fixedRemedies.Select(r => new RemedyDto() { Diagnosis = r.Diagnosis, Name = r.Name, IsFixed = r.IsFixed }));

                if (!request.FixedOnly && _dbcs.TryGetIndex(context.UserState["tenant"].ToString()!, out DbIndex tenantDb))
                {
                    reply.Remedies.AddRange(DatabaseReader.GetTenantRemedies(tenantDb));
                }
            }

            return Task.FromResult(reply);
        }

        public override Task<PatientReply> GetPatientsForPractice(PatientRequest request, ServerCallContext context)
        {
            var reply = new PatientReply();

            if (_dbcs.TryGetIndex("master", out DbIndex master))
            {
                reply.Patients.AddRange(DatabaseReader.GetPatients(master, request.PracticeIk));
            }

            return Task.FromResult(reply);
        }

        public override Task<AppointmentReply> GetAppointmentsForPatientAtPractice(AppointmentRequest request, ServerCallContext context)
        {
            var reply = new AppointmentReply();

            if (_dbcs.TryGetIndex(context.UserState["tenant"].ToString()!, out DbIndex tenantDb)) //crashes if missing => intended behaviour, the interceptor fucked up
            {
                reply.Appointments.AddRange(
                    DatabaseReader.GetAppointmentsForPatientForPractice(_dbcs.MasterIndex, tenantDb, request.PatientKv, request.PracticeIk));
            }
                
            return Task.FromResult(reply);
        }

        public override Task<AppointmentReply> GetAppointmentsForTherapist(AppointmentRequest request, ServerCallContext context)
        {
            var reply = new AppointmentReply();

            if (_dbcs.TryGetIndex(context.UserState["tenant"].ToString()!, out DbIndex tenantDb)) //crashes if missing => intended behaviour, the interceptor fucked up
            {
                reply.Appointments.AddRange(
                    DatabaseReader.GetAppointmentsForTherapist(_dbcs.MasterIndex, tenantDb, request.TherapistId));
            }

            return Task.FromResult(reply);
        }
    }
}
