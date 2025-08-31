using Grpc.Net.Client;
using MixedDbDistributionGrpcClient;
using System.Threading.Tasks;

namespace MixedDbDistributionGrpcClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:7103");
            var accessorClient = new Accessor.AccessorClient(channel);
            var reply = await accessorClient.PingAsync(new PingRequest() { Payload = "Test" });

            Console.WriteLine(reply.Message);
            Console.ReadKey();
        }
    }
}
