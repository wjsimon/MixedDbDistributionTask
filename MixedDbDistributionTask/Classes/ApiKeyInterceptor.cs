using Grpc.Core;
using Grpc.Core.Interceptors;
using MixedDbDistributionTask.Services;

namespace MixedDbDistributionTask.Classes
{
    internal class ApiKeyInterceptor : Interceptor
    {
        public ApiKeyInterceptor(
            ILogger<ApiKeyInterceptor> logger,
            DatabaseCreationService dbcs)
        {
            _logger = logger;
            _dbcs = dbcs;
        }

        private readonly ILogger _logger;
        private readonly DatabaseCreationService _dbcs;

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

                //manually bypass where no api key required at all; make internal lookup for "public" api methods + IsPublic(context) method?
                //this is, conceptually, somewhat a doubling if the ApiAllowance... maybe this isn't actually needed?
                if (context.Method.StartsWith("/databases.DatabaseManager") || 
                    context.Method == "/accessor.Accessor/GetRemedies" || 
                    context.Method == "/accessor.Accessor/Ping")
                {
                    return await continuation(request, context);
                }
                else if (key != null && DatabaseReader.CheckApiKey(_dbcs.MasterIndex, key.Value, out string[] grants))
                {
                    context.UserState.Add("grants", grants);
                    return await continuation(request, context);
                }
                else
                {
                    context.Status = new Status(StatusCode.Unauthenticated, "required api key missing or invalid");
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
