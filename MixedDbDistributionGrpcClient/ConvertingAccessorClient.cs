namespace MixedDbDistributionGrpcClient
{
    internal class ConvertingAccessorClient
    {
        public ConvertingAccessorClient(Accessor.AccessorClient grpcClient) 
        {
            _client = grpcClient;
        }

        private readonly Accessor.AccessorClient _client;
    }
}
