using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Authorization;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Services;

namespace MixedDbDistributionTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Services.AddControllers();
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
                var masterDb = dbcs.CreateMasterDb(loc);
                var hillsideDb = dbcs.CreateTenantDb(loc, "hillsidesumo");

                var practices = new Practice[]
                {
                    new Practice("practice1", "Practice #1", "The Practice Company"),
                    new Practice("practice2", "Leaf and Machine", "The Practice Company"),
                    new Practice("pratice3", "Not a Practice", "Some Competition")
                };

                //debug insertions for population
                dbcs.InsertPractices(masterDb, practices);
            }
            else { return; }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            //app.MapControllers();

            app.MapGrpcService<AccessorService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }

        public class ApiKeyServerInterceptor : Interceptor
        {
            private readonly ILogger _logger;

            public ApiKeyServerInterceptor(ILogger<ApiKeyServerInterceptor> logger)
            {
                _logger = logger;
            }

            public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
                TRequest request,
                ServerCallContext context,
                UnaryServerMethod<TRequest, TResponse> continuation)
            {
                _logger.LogInformation("Starting receiving call. Type/Method: {Type} / {Method}",
                    MethodType.Unary, context.Method);
                try
                {
                    var key = context.RequestHeaders.Get("api-key");
                    if (key != null)
                    {
                        if (key.Value == "ACCESS_TOKEN")
                        {
                            return await continuation(request, context);
                        }
                        else
                        {
                            context.Status = new Status(StatusCode.Unauthenticated, "Ungültiger API-Schlüssel.");
                            return default; //what to use instead?
                        }
                    }
                    else
                    {
                        context.Status = new Status(StatusCode.Unauthenticated, "API-Schlüssel ist erforderlich.");
                        return default;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error thrown by {context.Method}.");
                    throw;
                }
            }
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
