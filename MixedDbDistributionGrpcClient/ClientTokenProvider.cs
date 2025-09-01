using Grpc.Core;

namespace MixedDbDistributionGrpcClient
{
    internal class ClientTokenProvider
    {
        public ClientTokenProvider() { }

        public string? GetToken(AuthInterceptorContext context)
        {
            if (context.MethodName == "GetRemedies") { return null; }
            else { return "ACCESS_TOKEN"; }
        }

        public string GetInvalidToken()
            => "INVALID";
    }
}