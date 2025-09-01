using MixedDbDistribution.Dashboard;
using MixedDbDistributionTask.Dashboard.Classes;
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
            
            _introduction = new(this);

            _ = LoadDatabaseAvailability(); //together with non-nullable intro, this leads to a flicker everytime the app starts :(    (loadinitialdata event structure to prevent)
        }

        private readonly Accessor.AccessorClient _accessorClient;
        private readonly DatabaseManager.DatabaseManagerClient _dbManagerClient;

        private bool _masterAvailable = false;
        private int _showDebugDataPrompt = 0;
        private Introduction _introduction;

        private ImmutableArray<string> _availableTenants = [];

        public event EventHandler? DatabaseAvailabilityChanged;

        public bool MasterAvailable => _masterAvailable;
        public int ShowDebugDataPrompt => _showDebugDataPrompt;
        public Introduction Introduction => _introduction;
        public ImmutableArray<string> AvailableTenants => _availableTenants;

        public async Task<bool> CreateMasterDatabase()
        {
            await _dbManagerClient.CreateMasterDatabaseAsync(new DatabaseCreationRequest());
            await LoadDatabaseAvailability();

            _showDebugDataPrompt = 1;
            return true;
        }

        public async Task<bool> CreateTenantDatabase(string tenantId)
        {
            await _dbManagerClient.CreateTenantDatabaseAsync(new DatabaseCreationRequest() { TenantId = tenantId });
            await LoadDatabaseAvailability();

            _showDebugDataPrompt = 2;
            return true;
        }

        public async Task<bool> GenerateDebugData(int selection)
        {
            var reply = await _dbManagerClient.GenerateDebugDataAsync(new GenerationRequest() { Selection = selection });
            return true;
        }

        public void DismissDebugDataPrompt()
        {
            _showDebugDataPrompt = 0;
        }

        private async Task LoadDatabaseAvailability()
        {
            var availability = await _dbManagerClient.GetDatabaseAvailabilityAsync(new DatabasesRequest());
            _masterAvailable = availability.MasterAvailable;
            _availableTenants = availability.AvailableDatabases.ToImmutableArray();

            if (!MasterAvailable)
            {
                _introduction = new(this);
            }

            DatabaseAvailabilityChanged?.Invoke(this, EventArgs.Empty); //this also invokes a re-render for the intro... not the best communication but works for now
        }
    }
}
