using Grpc.Net.Client;

namespace MixedDbDistributionGrpcClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:7103");
            var accessorClient = new Accessor.AccessorClient(channel);
            var reply = await accessorClient.GetPracticesAsync(new PracticesRequest());

            var practices = reply.Practices;
            Console.ReadKey();
        }
    }
}