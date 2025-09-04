using Grpc.Core;
using MixedDbDistribution.Dashboard;
using MixedDbDistributionTask.Dashboard.Classes;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

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

            _ = LoadInitialData(); //together with non-nullable intro, this leads to a flicker everytime the app starts :(    (loadinitialdata event structure to prevent)
        }

        private readonly Accessor.AccessorClient _accessorClient;
        private readonly DatabaseManager.DatabaseManagerClient _dbManagerClient;

        private const string QUERY_FIXED_REMEDIES = "fixed-remedies";
        private const string QUERY_PRACTICES = "practices";
        private const string QUERY_PFORP = "patients-for-a-practice";
        private const string QUERY_APPOINTMENTS_PFORP = "appointments-for-a-patient-for-a-practice";
        private const string QUERY_THERAPISTS = "therapists";
        private const string QUERY_APPOINTMENTS_THERAPIST = "appointments-for-a-therapist";

        //any way to automatically resolve these?
        private ReadOnlyDictionary<string, QueryInfo> _availableQueries = new ReadOnlyDictionary<string, QueryInfo>(
            new Dictionary<string, QueryInfo>()
            {
                { QUERY_FIXED_REMEDIES, new QueryInfo(QUERY_FIXED_REMEDIES, [], QueryInfoScope.Master) },
                { QUERY_PRACTICES, new QueryInfo(QUERY_PRACTICES, [], QueryInfoScope.Master) },
                { QUERY_PFORP, new QueryInfo(QUERY_PFORP, ["practiceIk"], QueryInfoScope.Master) },
                { QUERY_THERAPISTS, new QueryInfo(QUERY_THERAPISTS, [], QueryInfoScope.Tenant) },
                { QUERY_APPOINTMENTS_PFORP, new QueryInfo(QUERY_APPOINTMENTS_PFORP, ["patientKv", "practiceIk"], QueryInfoScope.Tenant) },
                { QUERY_APPOINTMENTS_THERAPIST, new QueryInfo(QUERY_APPOINTMENTS_THERAPIST, ["therapistId"], QueryInfoScope.Tenant) }
            });

        private bool _masterAvailable = false;
        private bool _initialLoadCompleted = false;
        private int _showDebugDataPrompt = 0;
        private Introduction _introduction;

        private string? _setupError = null;
        private string? _selectedDatabase = null;
        private string? _selectedQuery = null;
        private string? _lastQuery = null;
        private string? _lastQueryResult = null;
        private string? _lastQueryResultFormatted = null;
        private string? _lastError = null;

        private ImmutableArray<string> _availableTenants = [];

        public event EventHandler? InitialLoadCompleted;
        public event EventHandler? DatabaseAvailabilityChanged;
        public event EventHandler<string?>? DatabaseSelectionChanged;
        public event EventHandler<int>? SelectedQueryParametersRequired;

        public bool ServiceReady => _initialLoadCompleted;
        public string? SetupError => _setupError;
        public bool MasterAvailable => _masterAvailable;
        public int ShowDebugDataPrompt => _showDebugDataPrompt;
        public Introduction Introduction => _introduction;
        public ImmutableArray<string> AvailableTenants => _availableTenants;

        public string? SelectedDatabase => _selectedDatabase;
        public string? ApiKey //teehee
        {
            get { return ClientTokenProvider.Token; }
            set { ClientTokenProvider.Token = value; }
        }

        public string? SelectedQuery => _selectedQuery;
        public string[] RequiredQueryParameters => GetRequiredQueryParamsSafe();
        public string? LastQuery => _lastQuery;
        public string? LastQueryResult => _lastQueryResult;
        public string? LastQueryResultFormatted => _lastQueryResultFormatted;
        public string? LastError => _lastError;

        public bool HasSelection => _selectedDatabase != null;
        public bool HasQuerySelection => _selectedQuery != null;
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

                _selectedQuery = null;
                ResetLastQuery();
                DatabaseSelectionChanged?.Invoke(this, _selectedDatabase);
            }
        }


        public async Task SelectQuery(string query)
        {
            if (!_availableQueries.ContainsKey(query)) { return; }

            _selectedQuery = query;
            if (_availableQueries[_selectedQuery].ParamIds.Length == 0)
            {
                await RequestQuery(_selectedQuery, []);
            }
            else
            {
                ResetLastQuery();
                SelectedQueryParametersRequired?.Invoke(this, _availableQueries[_selectedQuery].ParamIds.Length);
            }
        }

        public string[] GetAvailableQueriesForSelection()
        {
            if (_selectedDatabase == null) { return []; }
            return GetAvailableQueries(_selectedDatabase);
        }

        public async Task RequestSelectedQuery(string?[] paramValues)
        {
            if (_selectedQuery == null) { return; }
            if (paramValues.Length != _availableQueries[_selectedQuery].ParamIds.Length) { return; }

            await RequestQuery(_selectedQuery, paramValues);
        }

        public bool IsDatabaseSelected(string dbKey)
            => dbKey == _selectedDatabase;

        public bool IsQuerySelected(string query)
            => query == _lastQuery && _lastQueryResult != null;

        private async Task LoadInitialData()
        {
            try
            {
                var ping = await _accessorClient.PingAsync(new PingRequest());
                await LoadDatabaseAvailability();
            }
            catch (RpcException rpcEx)
            {
                if (rpcEx.Message.Contains("Failed to fetch"))
                {
                    _setupError = "Server unavailable. Check your configs and restart.";
                }
            }
            catch (Exception ex)
            {
                _setupError = ex.Message;
            }
            finally
            {
                if (!_initialLoadCompleted)
                {
                    _initialLoadCompleted = true;
                }

                InitialLoadCompleted?.Invoke(this, EventArgs.Empty);
            }


        }
        private async Task LoadDatabaseAvailability()
        {
            var availability = await _dbManagerClient.GetDatabaseAvailabilityAsync(new DatabasesRequest());
            _masterAvailable = availability.MasterAvailable;
            _availableTenants = availability.AvailableDatabases.ToImmutableArray();

            DatabaseAvailabilityChanged?.Invoke(this, EventArgs.Empty); //this also invokes a re-render for the intro... not the best communication but works for now
        }

        private string[] GetAvailableQueries(string dbName)
        {
            if (!_availableTenants.Contains(dbName)) { return []; }

            if (dbName == "master")
            {
                return _availableQueries.Values.Where(qi => qi.Scope == QueryInfoScope.Master).Select(qi => qi.Id).ToArray();
            }
            else
            {
                return _availableQueries.Values.Where(qi => qi.Scope == QueryInfoScope.Tenant).Select(qi => qi.Id).ToArray();
            }
        }

        private async Task RequestQuery(string query, string?[] paramValues)
        {
            if (!_availableQueries.ContainsKey(query)) { return; }

            _lastError = null;
            _lastQuery = query;

            try
            {
                if (_lastQuery == QUERY_FIXED_REMEDIES)
                {
                    var reply = await _accessorClient.GetRemediesAsync(new RemedyRequest() { FixedOnly = true });
                    _lastQueryResult = reply.ToString();
                }
                else if (_lastQuery == QUERY_PRACTICES)
                {
                    var reply = await _accessorClient.GetPracticesAsync(new PracticesRequest());
                    _lastQueryResult = reply.ToString();
                }
                else if (_lastQuery == QUERY_PFORP)
                {
                    var reply = await _accessorClient.GetPatientsForPracticeAsync(new PatientsRequest() { PracticeIk = paramValues[0] });
                    _lastQueryResult = reply.ToString();
                }
                else if (_lastQuery == QUERY_APPOINTMENTS_PFORP)
                {
                    var reply = await _accessorClient.GetAppointmentsForPatientAtPracticeAsync(
                        new AppointmentRequest() { PatientKv = paramValues[0], PracticeIk = paramValues[1] });

                    _lastQueryResult = reply.ToString();
                }
                else if (_lastQuery == QUERY_THERAPISTS)
                {
                    var replay = await _accessorClient.GetTherapistsAsync(new TherapistsRequest());
                    _lastQueryResult = replay.ToString();
                }
                else if (_lastQuery == QUERY_APPOINTMENTS_THERAPIST)
                {
                    var reply = await _accessorClient.GetAppointmentsForTherapistAsync(
                        new AppointmentRequest() { TherapistId = paramValues[0] });

                    _lastQueryResult = reply.ToString();
                }
                else
                {
                    ResetLastQuery();
                }
            }
            catch (Exception ex)
            {
                ResetLastQuery();
                _lastError = ex.ToString();
            }
        }

        private string[] GetRequiredQueryParamsSafe()
        {
            if (_selectedQuery == null) { return []; }

            if (_availableQueries.TryGetValue(_selectedQuery, out QueryInfo queryInfo))
            {
                return queryInfo.ParamIds;
            }
            else
            {
                return [];
            }
        }

        private void ResetLastQuery()
        {
            _lastQuery = null;
            _lastQueryResult = null;
            _lastQueryResultFormatted = null;
            _lastError = null;
        }
    }
}