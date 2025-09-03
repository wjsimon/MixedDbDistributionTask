using MixedDbDistributionTask.Classes;
using MixedDbDistributionTask.Services;

namespace MixedDbDistributionTask
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "_myAllowSpecificOrigins",
                                  policy =>
                                  {
                                      policy.WithOrigins("https://localhost:7195")
                                          .AllowCredentials()
                                          .AllowAnyHeader()
                                          .AllowAnyMethod();
                                  });
            });

            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<ApiKeyInterceptor>();
            });

            builder.AddServices();

            var app = builder.Build();

            app.UseRouting();

            app.UseCors("_myAllowSpecificOrigins");
            app.UseGrpcWeb();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGrpcService<AccessorService>().EnableGrpcWeb();
            app.MapGrpcService<DatabaseManagerService>().EnableGrpcWeb();

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