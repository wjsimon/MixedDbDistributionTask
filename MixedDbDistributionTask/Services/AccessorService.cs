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
        
        public override Task<PracticesReply> GetPractices(PracticesRequest request, ServerCallContext context)
        {
            var reply = new PracticesReply();

            if (_dbcs.TryGetIndex("master", out DbIndex index))
            {
                var practices = DatabaseReader.GetPractices(index);
                reply.Practices.AddRange(practices.Select(p => new PracticeDto() { Ik = p.Ik, Name = p.Name, Company = p.Company }));
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
                    var remedies = DatabaseReader.GetTenantRemedies(tenantDb);
                    reply.Remedies.AddRange(remedies.Select(r => new RemedyDto() { Diagnosis = r.Diagnosis, Name = r.Name, IsFixed = r.IsFixed }));
                }
            }

            return Task.FromResult(reply);
        }

        public override Task<PatientReply> GetPatientsForPractice(PatientRequest request, ServerCallContext context)
        {
            var reply = new PatientReply();

            if (_dbcs.TryGetIndex("master", out DbIndex master))
            {
                var patients = DatabaseReader.GetPatients(master, request.PracticeIk);
                reply.Patients.AddRange(patients.Select(p => new PatientDto() { KvNummer = p.KvNummer, PracticeIk = p.Practice.Ik, Name = p.Name, Age = p.Age }));
            }

            return Task.FromResult(reply);
        }

        public override Task<AppointmentReply> GetAppointmentsForPatientAtPractice(AppointmentRequest request, ServerCallContext context)
        {
            var reply = new AppointmentReply();

            if (_dbcs.TryGetIndex(context.UserState["tenant"].ToString()!, out DbIndex tenantDb)) //crashes if missing => intended behaviour, the interceptor fucked up
            {
                var appointments = DatabaseReader.GetAppointmentsForPatientForPractice(_dbcs.MasterIndex, tenantDb, request.PatientKv, request.PracticeIk);
                reply.Appointments.AddRange(appointments.Select(a => new AppointmentDto()
                {
                    Id = a.Id, 
                    StartTime = a.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    EndTime = a.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    TherapistId = a.Therapist.Id,
                    PatientKv = a.Patient.KvNummer,
                    PracticeIk = a.Practice.Ik,
                    RemedyDiagnosis = a.Remedy.Diagnosis
                }));
            }
                
            return Task.FromResult(reply);
        }

        public override Task<AppointmentReply> GetAppointmentsForTherapist(AppointmentRequest request, ServerCallContext context)
        {
            var reply = new AppointmentReply();

            if (_dbcs.TryGetIndex(context.UserState["tenant"].ToString()!, out DbIndex tenantDb)) //crashes if missing => intended behaviour, the interceptor fucked up
            {
                var appointments = DatabaseReader.GetAppointmentsForTherapist(_dbcs.MasterIndex, tenantDb, request.TherapistId);
                reply.Appointments.AddRange(appointments.Select(a => new AppointmentDto()
                {
                    Id = a.Id,
                    StartTime = a.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    EndTime = a.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    TherapistId = a.Therapist.Id,
                    PatientKv = a.Patient.KvNummer,
                    PracticeIk = a.Practice.Ik,
                    RemedyDiagnosis = a.Remedy.Diagnosis
                }));
            }

            return Task.FromResult(reply);
        }
    }
}
