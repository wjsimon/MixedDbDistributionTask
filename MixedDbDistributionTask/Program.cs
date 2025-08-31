using MixedDbDistributionTask.Services;
using MixedDbDistributionTask.Classes;
using MixedDbDistributionTask.Data;

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

                //var practices = new Practice[]
                //{
                //    new Practice("practice1", "Practice #1", "The Practice Company"),
                //    new Practice("practice2", "Leaf and Machine", "The Practice Company"),
                //    new Practice("pratice3", "Not a Practice", "Some Competition")
                //};

                ////debug insertions for population
                //dbcs.InsertPractices(masterDb, practices);

                //var fixedRemedies = new Remedy[]
                //{
                //    new Remedy("bad", "The Bad One", true),
                //    new Remedy("even worse", "Wouldn't want to be you", true),
                //    new Remedy("good", "All good buddy", true)
                //};

                //dbcs.InsertRemedies(masterDb, fixedRemedies);

                //var patients = new Patient[]
                //{
                //    new Patient("0", "practice1", "Wilhelm Simon", 29),
                //    new Patient("1", "practice1", "Hannes Roever", -1),
                //    new Patient("2", "practice2", "Raphael Schweda", -1)
                //};

                //dbcs.InsertPatients(masterDb, patients);
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