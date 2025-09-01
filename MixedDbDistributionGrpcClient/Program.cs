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
            var databases = await accessorClient.GetDatabaseAvailabilityAsync(new DatabasesRequest());
            var practicesReply = await accessorClient.GetPracticesAsync(new PracticesRequest());
            var remediesReply = await accessorClient.GetRemediesAsync(new RemedyRequest() { FixedOnly = true });
            var patientsReply = await accessorClient.GetPatientsForPracticeAsync(new PatientRequest() { PracticeIk = "practice1" });
            var appointmentsForPatientsForPractice = await accessorClient.GetAppointmentsForPatientAtPracticeAsync(new AppointmentRequest() { PatientKv = "0", PracticeIk = "practice1" });
            var appointmentsForTherapist = await accessorClient.GetAppointmentsForTherapistAsync(new AppointmentRequest() { TherapistId = "therapist1" });

            var availability = databases.AvailableDatabases;
            var practices = practicesReply.Practices;
            var remedies = remediesReply.Remedies;
            var patients = patientsReply.Patients;
            var appointments1 = appointmentsForPatientsForPractice.Appointments;
            var appointments2 = appointmentsForTherapist.Appointments;

            Console.ReadKey();
        }

        private static GrpcChannel CreateAuthenticatedChannel(ClientTokenProvider tokenProvider)
        {
            var credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
            {
                var token = tokenProvider.GetToken(context);
                if (token != null)
                {
                    metadata.Add("api-key", $"{token}");
                }
            });

            var channel = GrpcChannel.ForAddress("https://localhost:7103", new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });

            return channel;
        }
    }
}