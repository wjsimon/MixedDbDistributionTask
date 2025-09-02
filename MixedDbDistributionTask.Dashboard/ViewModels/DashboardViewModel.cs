using Google.Protobuf.WellKnownTypes;
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
        
        //any way to automatically resolve these?
        private readonly ImmutableArray<string> _availableQueries = [
            "fixed-remedies",
            "practices",
            "patients-for-a-practice",
            "appointments-for-a-patient-for-a-practice",
            "appointments-for-a-therapist"
        ];

        private bool _masterAvailable = false;
        private bool _fistLoadCompleted = false;
        private int _showDebugDataPrompt = 0;
        private Introduction _introduction;

        private string? _selectedDatabase = null;
        private string? _lastQuery = null;
        private string? _lastQueryResult = null;
        private string? _lastQueryResultFormatted = null;

        private ImmutableArray<string> _availableTenants = [];

        public event EventHandler? InitialLoadCompleted;
        public event EventHandler? DatabaseAvailabilityChanged;
        public event EventHandler<string?>? DatabaseSelectionChanged;

        public bool ServiceReady => _fistLoadCompleted;
        public bool MasterAvailable => _masterAvailable;
        public int ShowDebugDataPrompt => _showDebugDataPrompt;
        public Introduction Introduction => _introduction;
        public ImmutableArray<string> AvailableTenants => _availableTenants;

        public string? SelectedDatabase => _selectedDatabase;
        public ImmutableArray<string> AvailableQueries => _availableQueries; //load from backend!
        public string? ApiKey { get; set; }
        public string? LastQuery => _lastQuery;
        public string? LastQueryResult => _lastQueryResult;
        public string? LastQueryResultFormatted => _lastQueryResultFormatted;

        public bool HasSelection => _selectedDatabase != null;
        public bool HasQueryResult => _lastQueryResult != null;

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

        public async Task<bool> GenerateDebugData(int selection, string[] tenants)
        {
            var request = new GenerationRequest() { Selection = selection };
            request.Tenants.AddRange(tenants);

            var reply = await _dbManagerClient.GenerateDebugDataAsync(request);
            return true;
        }

        public void DismissDebugDataPrompt()
        {
            _showDebugDataPrompt = 0;
        }

        public void SelectDatabase(string dbKey)
        {
            if (_availableTenants.Contains(dbKey))
            {
                if (_selectedDatabase == dbKey)
                {
                    _selectedDatabase = null;
                }
                else
                {
                    _selectedDatabase = dbKey;
                }

                ResetQuery();
                DatabaseSelectionChanged?.Invoke(this, _selectedDatabase);
            }
        }

        public async Task RequestQuery(int queryIndex) {
            //api call
            if (queryIndex-1 > _availableQueries.Length) { return; }

            _lastQuery = _availableQueries[queryIndex];
            if (_lastQuery == "fixed-remedies")
            {
                var reply = await _accessorClient.GetRemediesAsync(new RemedyRequest() { FixedOnly = true });
                _lastQueryResult = reply.ToString();
            }
            else
            {
                _lastQuery = null;
                _lastQueryResult = null; 
            }
        }

        public bool IsDatabaseSelected(string dbKey) 
            => dbKey == _selectedDatabase;

        public bool IsQuerySelected(string query)
            => query == _lastQuery && _lastQueryResult != null;

        private async Task LoadDatabaseAvailability()
        {
            var availability = await _dbManagerClient.GetDatabaseAvailabilityAsync(new DatabasesRequest());
            _masterAvailable = availability.MasterAvailable;
            _availableTenants = availability.AvailableDatabases.ToImmutableArray();

            if (!MasterAvailable)
            {
                _introduction = new(this);
            }

            if (!_fistLoadCompleted)
            {
                _fistLoadCompleted = true;
                InitialLoadCompleted?.Invoke(this, EventArgs.Empty);
            }

            DatabaseAvailabilityChanged?.Invoke(this, EventArgs.Empty); //this also invokes a re-render for the intro... not the best communication but works for now
        }

        private void ResetQuery()
        {
            _lastQuery = null;
            _lastQueryResult = null;
            _lastQueryResultFormatted = null;
        }
    }
}
