using Grpc.Core;
using Grpc.Net.Client;

namespace MixedDbDistributionGrpcClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = CreateAuthenticatedChannel(new ClientTokenProvider()); //replace with DI

            var accessorClient = new Accessor.AccessorClient(channel);
            var practicesReply = await accessorClient.GetPracticesAsync(new PracticesRequest());
            var remediesReply = await accessorClient.GetRemediesAsync(new RemedyRequest());
            var patientsReply = await accessorClient.GetPatientsForPracticeAsync(new PatientRequest() { PracticeIk = "practice1" });

            var practices = practicesReply.Practices;
            var remedies = remediesReply.Remedies;
            var patients = patientsReply.Patients;

            Console.ReadKey();
        }

        private static GrpcChannel CreateAuthenticatedChannel(ClientTokenProvider tokenProvider)
        {
            var credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
            {
                var token = tokenProvider.GetToken();
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