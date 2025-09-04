using Grpc.Core;
using Grpc.Net.Client;
using MixedDbDistributionGrpcClient;

namespace MixedDbDistributionTask.QuickStart
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = CreateAuthenticatedChannel();

            var accessorClient = new Accessor.AccessorClient(channel);
            var adminClient = new DatabaseManager.DatabaseManagerClient(channel);

            await adminClient.CreateMasterDatabaseAsync(new DatabaseCreationRequest());
            await adminClient.CreateTenantDatabaseAsync(new DatabaseCreationRequest() { TenantId = "henara" });
            await adminClient.CreateTenantDatabaseAsync(new DatabaseCreationRequest() { TenantId = "simonssoftware" });

            var genReq = new GenerationRequest() { Selection = 3 };
            genReq.Tenants.AddRange(["henara", "simonssoftware"]);

            await adminClient.GenerateDebugDataAsync(genReq);

            //lil test run
            var databases = await adminClient.GetDatabaseAvailabilityAsync(new DatabasesRequest());
            var create = await adminClient.CreateMasterDatabaseAsync(new DatabaseCreationRequest());
            var practicesReply = await accessorClient.GetPracticesAsync(new PracticesRequest());
            var remediesReply = await accessorClient.GetRemediesAsync(new RemedyRequest() { FixedOnly = true });
            var patientsReply = await accessorClient.GetPatientsForPracticeAsync(new PatientsRequest() { PracticeIk = "practice1" });
            var appointmentsForPatientsForPractice = await accessorClient.GetAppointmentsForPatientAtPracticeAsync(new AppointmentRequest() { PatientKv = "0", PracticeIk = "practice1", Tenant = "henara" });
            var appointmentsForTherapist = await accessorClient.GetAppointmentsForTherapistAsync(new AppointmentRequest() { TherapistId = "therapist1", Tenant = "simonssoftware" });

            var practices = practicesReply.Practices;
            var remedies = remediesReply.Remedies;
            var patients = patientsReply.Patients;
            var appointments1 = appointmentsForPatientsForPractice.Appointments;
            var appointments2 = appointmentsForTherapist.Appointments;

            //just set a breakpoint here to check the data
            Console.ReadKey();
        }

        private static GrpcChannel CreateAuthenticatedChannel()
        {
            var credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
            {
                var token = ClientTokenProvider.Token;
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