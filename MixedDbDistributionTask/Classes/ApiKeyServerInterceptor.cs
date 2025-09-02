using Grpc.Core;
using Grpc.Core.Interceptors;

namespace MixedDbDistributionTask.Classes
{
    public class ApiKeyServerInterceptor : Interceptor
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _validApiKeys = new Dictionary<string, string>()
        {
            { "ACCESS_TOKEN", "henara" }
        };

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

                //no api key required
                if (context.Method == "/accessor.Accessor/GetRemedies") //make internal lookup for "public" api methods + IsPublic(context) method?
                {
                    return await continuation(request, context);
                }
                else if (key != null && _validApiKeys.TryGetValue(key.Value, out string? tenantId))
                {
                    context.UserState.Add("tenant", tenantId);
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
