using Grpc.Core;
using MixedDbDistributionTask.Data;

namespace MixedDbDistributionTask.Services
{
    internal class DatabaseManagerService : DatabaseManager.DatabaseManagerBase
    {
        public DatabaseManagerService(
            ILogger<AccessorService> logger,
            IConfiguration configuration,
            DatabaseCreationService dbcs)
        {
            _logger = logger;
            _configuration = configuration;
            _dbcs = dbcs;
        }

        private readonly ILogger<AccessorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly DatabaseCreationService _dbcs;

        public override Task<DatabasesReply> GetDatabaseAvailability(DatabasesRequest request, ServerCallContext context)
        {
            var reply = new DatabasesReply();
            reply.MasterAvailable = _dbcs.MasterAvailable;
            reply.AvailableDatabases.AddRange(_dbcs.GetAvailableDatabases());

            return Task.FromResult(reply);
        }

        public override Task<DatabaseCreationReply> CreateMasterDatabase(DatabaseCreationRequest request, ServerCallContext context)
        {
            var reply = new DatabaseCreationReply();

            if (!_dbcs.MasterAvailable)
            {
                var loc = _configuration["ConnectionStrings:SqliteMasterDeb"];
                if (loc == null) { throw new Exception("Server configuration invalid. No sqlite connection string present."); }

                _dbcs.CreateMasterDbSafe(loc);
            }

            return Task.FromResult(reply);
        }

        public override Task<DatabaseCreationReply> CreateTenantDatabase(DatabaseCreationRequest request, ServerCallContext context)
        {
            var reply = new DatabaseCreationReply();

            if (!_dbcs.TryGetIndex(request.TenantId, out DbIndex index))
            {
                var loc = _configuration["ConnectionStrings:SqliteMasterDeb"];
                if (loc == null) { throw new Exception("Server configuration invalid. No sqlite connection string present."); }

                _dbcs.CreateTenantDbSafe(loc, request.TenantId);
            }

            return Task.FromResult(reply);
        }

        public override Task<GenerationReply> GenerateDebugData(GenerationRequest request, ServerCallContext context)
        {
            var reply = new GenerationReply();

            if ((request.Selection & (1 << 0)) != 0)
            {
                if (_dbcs.MasterAvailable)
                {
                    _dbcs.GenerateMasterDebugData(_dbcs.MasterIndex);
                }
            }

            if ((request.Selection & (1 << 1)) != 0)
            {
                foreach (var tenantId in request.Tenants)
                {
                    if (_dbcs.TryGetIndex(tenantId, out DbIndex tenantIndex))
                    {
                        _dbcs.GenerateTenantDebugData(tenantIndex, tenantId);
                    }
                }
            }

            return Task.FromResult(reply);
        }
    }
}
