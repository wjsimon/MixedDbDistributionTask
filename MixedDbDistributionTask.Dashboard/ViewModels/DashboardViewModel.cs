using MixedDbDistributionGrpcClient;
using System.Collections.Immutable;

namespace MixedDbDistributionTask.Dashboard.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardViewModel(Accessor.AccessorClient client) 
        {
            _client = client;

            _ = LoadDatabaseAvailability();
        }

        private readonly Accessor.AccessorClient _client;

        private bool _masterAvailable = false;
        private ImmutableArray<string> _availableTenants = [];

        public event EventHandler? DatabaseAvailabilityChanged;

        public bool MasterAvailable => _masterAvailable;
        public ImmutableArray<string> AvailableTenants => _availableTenants;

        public async Task<bool> CreateMasterDatabase()
        {
            return false;
        }

        private async Task LoadDatabaseAvailability()
        {
            var availability = await _client.GetDatabaseAvailabilityAsync(new DatabasesRequest());
            _masterAvailable = availability.MasterAvailable;
            _availableTenants = availability.AvailableDatabases.ToImmutableArray();

            DatabaseAvailabilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
