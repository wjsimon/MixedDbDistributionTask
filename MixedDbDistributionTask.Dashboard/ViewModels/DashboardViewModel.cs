using MixedDbDistribution.Dashboard;
using System.Collections.Immutable;

namespace MixedDbDistributionTask.Dashboard.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardViewModel(
            Accessor.AccessorClient accessorClient,
            DatabaseManager.DatabaseManagerClient dbManagerClient) 
        {
            _accessorClient = accessorClient;
            _dbManagerClient = dbManagerClient;

            _ = LoadDatabaseAvailability();
        }

        private readonly Accessor.AccessorClient _accessorClient;
        private readonly DatabaseManager.DatabaseManagerClient _dbManagerClient;

        private bool _masterAvailable = false;
        private bool _showDebugDataPrompt = false;
        private ImmutableArray<string> _availableTenants = [];

        public event EventHandler? DatabaseAvailabilityChanged;

        public bool MasterAvailable => _masterAvailable;
        public bool ShowDebugDataPrompt => _showDebugDataPrompt;
        public ImmutableArray<string> AvailableTenants => _availableTenants;

        public async Task<bool> CreateMasterDatabase()
        {
            await _dbManagerClient.CreateMasterDatabaseAsync(new DatabaseCreationRequest());
            await LoadDatabaseAvailability();

            _showDebugDataPrompt = true;
            return true;
        }

        public async Task<bool> CreateTenantDatabase(string tenantId)
        {
            await _dbManagerClient.CreateTenantDatabaseAsync(new DatabaseCreationRequest() { TenantId = tenantId });
            await LoadDatabaseAvailability();

            return true;
        }

        public async Task<bool> GenerateDebugData(int selection)
        {
            var reply = await _dbManagerClient.GenerateDebugDataAsync(new GenerationRequest() { Selection = selection });
            return true;
        }

        public void DismissDebugDataPrompt()
        {
            _showDebugDataPrompt = false;
        }

        private async Task LoadDatabaseAvailability()
        {
            var availability = await _dbManagerClient.GetDatabaseAvailabilityAsync(new DatabasesRequest());
            _masterAvailable = availability.MasterAvailable;
            _availableTenants = availability.AvailableDatabases.ToImmutableArray();

            DatabaseAvailabilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
