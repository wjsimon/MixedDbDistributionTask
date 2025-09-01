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
                if(dbcs.CreateMasterDbSafe(loc))
                {
                    dbcs.WriteMasterDebugData(dbcs.MasterIndex);
                }
                
                if(dbcs.CreateTenantDbSafe(loc, "henara"))
                {
                    dbcs.WriteTenantDebugData(dbcs.GetIndex("henara"));
                }
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