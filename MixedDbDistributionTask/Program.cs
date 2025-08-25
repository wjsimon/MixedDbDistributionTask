using MixedDbDistributionTask.Services;

namespace MixedDbDistributionTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.AddServices();

            var app = builder.Build();

            var config = app.Services.GetRequiredService<IConfiguration>();
            var dbcs = app.Services.GetRequiredService<DatabaseCreationService>();

            var loc = config["ConnectionStrings:SqliteMasterDeb"];

            if (loc != null) 
            {
                dbcs.CreateMasterDb(loc);
                dbcs.CreateTenantDb(loc, "hillsidesumo"); 
            }
            else { return; }

            return;

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

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
