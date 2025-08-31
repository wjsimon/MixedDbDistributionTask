namespace MixedDbDistributionGrpcClient
{
    internal class ClientTokenProvider
    {
        public ClientTokenProvider() { }

        public string GetToken()
            => "ACCESS_TOKEN";

        public string GetInvalidToken()
            => "INVALID";
    }
}