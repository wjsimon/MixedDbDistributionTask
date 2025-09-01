using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MixedDbDistributionGrpcClient;
using MixedDbDistributionTask.Dashboard.Classes;
using MixedDbDistributionTask.Dashboard.ViewModels;

namespace MixedDbDistributionTask.Dashboard
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<DashboardViewModel>();

            builder.Services.AddSingleton<Accessor.AccessorClient>(CreateAccessorClient(builder.Configuration));

            await builder.Build().RunAsync();
        }

        private static Accessor.AccessorClient CreateAccessorClient(IConfiguration config)
            => new Accessor.AccessorClient(CreateAuthenticatedChannel(new ClientTokenProvider(), config)); //token from config?

        private static GrpcChannel CreateAuthenticatedChannel(ClientTokenProvider tokenProvider, IConfiguration config)
        {
            try
            {
                var host = config["host"];
                if (host == null) { throw new NullReferenceException($"app host is not configured"); }

                var credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
                {
                    var token = tokenProvider.GetToken(context);
                    if (token != null)
                    {
                        metadata.Add("api-key", $"{token}");
                    }
                });


                var channel = GrpcChannel.ForAddress(host, new GrpcChannelOptions
                {
                    HttpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler())),
                    Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
                });

                return channel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}