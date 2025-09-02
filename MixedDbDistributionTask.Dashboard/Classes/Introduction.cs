using MixedDbDistributionTask.Dashboard.ViewModels;

namespace MixedDbDistributionTask.Dashboard.Classes
{
    public class Introduction(DashboardViewModel dashboardViewModel)
    {
        private readonly DashboardViewModel _dashboardViewModel = dashboardViewModel;

        private int _stage = 0;
        private object[] _args = new object[4];

        public bool Show { get; set; }
        public int Stage => _stage;
        public object this[int index] => _args[index];

        public bool IsCompleted => Stage > 3;

        public Task? CompletionTask = null;
        public event EventHandler? Completed;

        public void Advance(object arg)
        {
            if (_args.Length > _stage)
            {
                _args[_stage] = arg;
            }

            _stage++;

            CompleteConditional();
        }

        public void Skip(object arg, int steps)
        {
            Advance(arg);
            CompleteConditional();
        }

        public void Dismiss()
        {
            _stage = 4;
            CompletionTask = null;

            Completed?.Invoke(this, EventArgs.Empty);
        }

        private void CompleteConditional()
        {
            if (IsCompleted)
            {
                CompletionTask = Task.Run(Complete);
            }
        }
        
        private async Task Complete()
        {
            //propagate into viewmodel methods
            await _dashboardViewModel.CreateMasterDatabase(); // <= _args[0]

            if ((bool)_args[1]! == true)
            {
                var tenants = ((string)_args[2]).Split(',');
                foreach (var tenant in tenants)
                {
                    await _dashboardViewModel.CreateTenantDatabase(tenant);
                }
            }

            var selection = (int)_args[3];
            if (selection > 0) 
            {
                var tenants = ((string)_args[2]).Split(',');
                await _dashboardViewModel.GenerateDebugData(selection, tenants);
            }

            Completed?.Invoke(this, EventArgs.Empty);
        }
    }
}