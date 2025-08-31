using MixedDbDistributionTask.Classes;
using MixedDbDistributionTask.Services;

namespace MixedDbDistributionTask
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<ApiKeyServerInterceptor>();
            });

            builder.AddServices();

            var app = builder.Build();

            var config = app.Services.GetRequiredService<IConfiguration>();
            var dbcs = app.Services.GetRequiredService<DatabaseCreationService>();

            var loc = config["ConnectionStrings:SqliteMasterDeb"];

            if (loc != null)
            {
                var masterDb = dbcs.CreateMasterDbSafe(loc);
                var hillsideDb = dbcs.CreateTenantDbSafe(loc, "hillsidesumo");

                //var practices = new PracticeDto[]
                //{
                //    new PracticeDto() { Ik = "practice1", Name = "Practice #1", Company = "The Practice Company" },
                //    new PracticeDto() { Ik = "practice2", Name = "Leaf and Machine", Company = "The Practice Company" },
                //    new PracticeDto() { Ik = "pratice3", Name = "Not a Practice", Company = "Some Competition" }
                //};

                ////debug insertions for population
                //DatabaseWriterService.InsertPractices(masterDb, practices);

                //var fixedRemedies = new RemedyDto[]
                //{
                //    new RemedyDto() { Diagnosis = "bad", Name = "The Bad One", IsFixed = true },
                //    new RemedyDto() { Diagnosis = "even worse", Name = "Wouldn't want to be you", IsFixed = true },
                //    new RemedyDto() { Diagnosis = "good", Name = "All good buddy", IsFixed = true }
                //};

                //DatabaseWriterService.InsertRemedies(masterDb, fixedRemedies);

                //var patients = new PatientDto[]
                //{
                //    new PatientDto() { KvNummer = "0", PracticeIk = "practice1", Name = "Wilhelm Simon", Age = 29 },
                //    new PatientDto() { KvNummer = "1", PracticeIk = "practice1", Name = "Hannes Roever", Age = -1 },
                //    new PatientDto() { KvNummer = "2", PracticeIk = "practice2", Name = "Raphael Schweda", Age = -1 }
                //};

                //DatabaseWriterService.InsertPatients(masterDb, patients);
            }
            else { return; }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGrpcService<AccessorService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }

    public static class ProgramExtensions
    {
        public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<DatabaseCreationService>();

            return builder;
        }
    }
}