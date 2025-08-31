using Grpc.Core;
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

            if (_dbcs.AvailableDatabases.TryGetValue("master", out DbIndex index))
            {
                var practices = DatabaseReaderService.GetPractices(index);
                reply.Practices.AddRange(practices.Select(p => new PracticeDto() { Ik = p.Ik, Name = p.Name, Company = p.Company }));
            }

            return Task.FromResult(reply);
        }

        public override Task<RemedyReply> GetRemedies(RemedyRequest request, ServerCallContext context)
        {
            var reply = new RemedyReply();
            
            if (_dbcs.AvailableDatabases.TryGetValue("master", out DbIndex master))
            {
                var fixedRemedies = DatabaseReaderService.GetFixedRemedies(master);
                reply.Remedies.AddRange(fixedRemedies.Select(r => new RemedyDto() { Diagnosis = r.Diagnosis, Name = r.Name, IsFixed = r.IsFixed }));

                if (!request.FixedOnly && _dbcs.AvailableDatabases.TryGetValue(context.UserState["tenant"].ToString()!, out DbIndex tenantDb))
                {
                    var remedies = DatabaseReaderService.GetTenantRemedies(tenantDb);
                    reply.Remedies.AddRange(remedies.Select(r => new RemedyDto() { Diagnosis = r.Diagnosis, Name = r.Name, IsFixed = r.IsFixed }));
                }
            }

            return Task.FromResult(reply);
        }

        public override Task<PatientReply> GetPatientsForPractice(PatientRequest request, ServerCallContext context)
        {
            var reply = new PatientReply();

            if (_dbcs.AvailableDatabases.TryGetValue("master", out DbIndex master))
            {
                var patients = DatabaseReaderService.GetPatients(master, request.PracticeIk);
                reply.Patients.AddRange(patients.Select(p => new PatientDto() { KvNummer = p.KvNummer, PracticeIk = p.Practice.Ik, Name = p.Name, Age = p.Age }));
            }

            return Task.FromResult(reply);
        }
    }
}
