using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace pscommander
{
    public class DataSourceService
    {
        private readonly List<DataSourceJob> _cronJobs = new List<DataSourceJob>();
        public readonly List<DataSource> DataSources = new List<DataSource>();
        private readonly PowerShellService _powerShellService;
        private readonly MenuService _menuService;
        private CancellationTokenSource _cancelSource;
        public DataSourceService(PowerShellService powerShellService, MenuService menuService)
        {
            _powerShellService = powerShellService;
            _menuService = menuService;
        }

        public void SetDataSources(IEnumerable<DataSource> dataSources)
        {
            if (_cancelSource != null)
            {
                _cancelSource.Cancel();
                _cancelSource = null;
            }
            
            _cancelSource = new CancellationTokenSource();

            foreach(var job in _cronJobs)
            {
                job.Dispose();
            }

            _cronJobs.Clear();
            DataSources.Clear();

            foreach(var dataSource in dataSources)
            {
                var job = new DataSourceJob(dataSource, _powerShellService, _menuService);
                job.StartAsync(_cancelSource.Token).Wait();
                _cronJobs.Add(job);
            }

            DataSources.AddRange(dataSources);
        }
    }

    public class DataSourceJob : IDisposable
    {
        private System.Timers.Timer _timer;
        private readonly DataSource _dataSource;
        private readonly PowerShellService _powerShellService;
        private readonly MenuService _menuService;

        public DataSourceJob(DataSource dataSource, PowerShellService powerShellService, MenuService menuService)
        {
            _dataSource = dataSource;
            _powerShellService = powerShellService;
            _menuService = menuService;
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            await DoWork(cancellationToken);
            await ScheduleJob(cancellationToken);
        }

        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
        {
            _timer = new System.Timers.Timer(_dataSource.RefreshInterval * 1000);
            _timer.Elapsed += async (sender, args) =>
            {
                _timer.Dispose();  // reset and dispose timer
                _timer = null;

                if (!cancellationToken.IsCancellationRequested)
                {
                    await DoWork(cancellationToken);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ScheduleJob(cancellationToken);    // reschedule next
                }
            };
            _timer.Start();
            await Task.CompletedTask;
        }

        public virtual async Task DoWork(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            try 
            {
                _dataSource.CurrentValue = _powerShellService.ExecuteNoUnwrap(_dataSource.LoadData, _dataSource.ArgumentList).First();
                if (_dataSource.History.Count == _dataSource.HistoryLimit)
                {
                    var key = _dataSource.History.Keys.OrderByDescending(m => m).First();
                    _dataSource.History.Remove(key);
                }

                _dataSource.History.Add(DateTime.Now, _dataSource.CurrentValue);
            }
            catch (Exception ex)
            {
                _menuService.ShowError(ex.Message);
            }
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            await Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            _timer?.Dispose();
        }
    }
}