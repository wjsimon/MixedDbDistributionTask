using Grpc.Core;
using Grpc.Net.Client;
using System.Net;

namespace MixedDbDistributionGrpcClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = CreateAuthenticatedChannel(new ClientTokenProvider()); //replace with DI

            var accessorClient = new Accessor.AccessorClient(channel);
            var reply = await accessorClient.GetPracticesAsync(new PracticesRequest());

            var practices = reply.Practices;
            Console.ReadKey();
        }

        private static GrpcChannel CreateAuthenticatedChannel(ClientTokenProvider tokenProvider)
        {
            var credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
            {
                var token = tokenProvider.GetInvalidToken();
                metadata.Add("api-key", $"{token}");
            });

            var channel = GrpcChannel.ForAddress("https://localhost:7103", new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });

            return channel;
        }
    }
}