using Grpc.Core;

namespace MixedDbDistributionTask.Services
{
    public class AccessorService : Accessor.AccessorBase
    {
        private readonly ILogger<AccessorService> _logger;
        public AccessorService(ILogger<AccessorService> logger)
        {
            _logger = logger;
        }

        public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingReply
            {
                Message = "Hello " + request.Payload
            });
        }
    }
}
