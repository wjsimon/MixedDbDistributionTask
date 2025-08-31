using Grpc.Core;
using MixedDbDistributionTask.Data;

namespace MixedDbDistributionTask.Services
{
    public class AccessorService : Accessor.AccessorBase
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
                var practices = _dbcs.GetPractices(index);
                reply.Practices.AddRange(practices.Select(p => new PracticeObj() { Ik = p.Ik, Name = p.Name, Company = p.Company }));
            }

            return Task.FromResult(reply);
        }

        public override Task<RemedyReply> GetRemedies(RemedyRequest request, ServerCallContext context)
        {
            var reply = new RemedyReply();
            
            if (_dbcs.AvailableDatabases.TryGetValue("master", out DbIndex master))
            {
                var fixedRemedies = _dbcs.GetFixedRemedies(master);
                reply.Remedies.AddRange(fixedRemedies.Select(r => new RemedyObj() { Diagnosis = r.Diagnosis, Name = r.Name, IsFixed = r.IsFixed }));

                if (!request.FixedOnly && _dbcs.AvailableDatabases.TryGetValue(context.UserState["tenant"].ToString()!, out DbIndex tenantDb))
                {
                    var remedies = _dbcs.GetTenantRemedies(tenantDb);
                    reply.Remedies.AddRange(remedies.Select(r => new RemedyObj() { Diagnosis = r.Diagnosis, Name = r.Name, IsFixed = r.IsFixed }));
                }
            }

            return Task.FromResult(reply);
        }
    }
}
