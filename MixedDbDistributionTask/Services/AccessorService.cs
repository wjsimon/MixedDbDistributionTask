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
    }
}
