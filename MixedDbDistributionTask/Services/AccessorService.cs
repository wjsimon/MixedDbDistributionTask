using Grpc.Core;
using MixedDbDistributionTask.Classes;
using MixedDbDistributionTask.Data;
using static MixedDbDistributionTask.Classes.ApiAllowance;

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

        public override Task<PracticesReply> GetPractices(PracticesRequest request, ServerCallContext context)
        {
            var reply = new PracticesReply();
            if (AllowGlobal(_dbcs, context, out DbIndex? master))
            {
                if (master == null) { throw new Exception("something went VERY wrong"); }
                reply.Practices.AddRange(DatabaseReader.GetPractices((DbIndex)master));
            }

            return Task.FromResult(reply);
        }

        public override Task<RemedyReply> GetRemedies(RemedyRequest request, ServerCallContext context)
        {
            var reply = new RemedyReply();

            if (AllowPublic(_dbcs, context, out DbIndex? master))
            {
                if (master == null) { throw new Exception("something went VERY wrong"); }
                var fixedRemedies = DatabaseReader.GetFixedRemedies((DbIndex)master);
                reply.Remedies.AddRange(fixedRemedies.Select(r => new RemedyDto() { Diagnosis = r.Diagnosis, Name = r.Name, IsFixed = r.IsFixed }));

                if (!request.FixedOnly && _dbcs.TryGetIndex(context.UserState["tenant"].ToString()!, out DbIndex tenantDb))
                {
                    reply.Remedies.AddRange(DatabaseReader.GetTenantRemedies(tenantDb));
                }
            }

            return Task.FromResult(reply);
        }

        public override Task<PatientsReply> GetPatientsForPractice(PatientsRequest request, ServerCallContext context)
        {
            var reply = new PatientsReply();
            if (AllowGlobal(_dbcs, context, out DbIndex? master))
            {
                if (master == null) { throw new Exception("something went VERY wrong"); }
                reply.Patients.AddRange(DatabaseReader.GetPatientsForPractice((DbIndex)master, request.PracticeIk));
            }

            return Task.FromResult(reply);
        }

        public override Task<AppointmentReply> GetAppointmentsForPatientAtPractice(AppointmentRequest request, ServerCallContext context)
        {
            var reply = new AppointmentReply();

            if (AllowLocal(_dbcs, context, out DbIndex tenantDb)) //crashes if missing => intended behaviour, the interceptor fucked up
            {
                reply.Appointments.AddRange(
                    DatabaseReader.GetAppointmentsForPatientForPractice(_dbcs.MasterIndex, tenantDb, request.PatientKv, request.PracticeIk));
            }
                
            return Task.FromResult(reply);
        }


        public override Task<TherapistsReply> GetTherapists(TherapistsRequest request, ServerCallContext context)
        {
            var reply = new TherapistsReply();

            if (AllowLocal(_dbcs, context, out DbIndex tenantDb))
            {
                reply.Therapists.AddRange(DatabaseReader.GetTherapists(tenantDb));
            }
            
            return Task.FromResult(reply);
        }

        public override Task<AppointmentReply> GetAppointmentsForTherapist(AppointmentRequest request, ServerCallContext context)
        {
            var reply = new AppointmentReply();

            if (AllowLocal(_dbcs, context, out DbIndex tenantDb)) //crashes if missing => intended behaviour, the interceptor fucked up
            {
                reply.Appointments.AddRange(
                    DatabaseReader.GetAppointmentsForTherapist(_dbcs.MasterIndex, tenantDb, request.TherapistId));
            }

            return Task.FromResult(reply);
        }
    }
}
