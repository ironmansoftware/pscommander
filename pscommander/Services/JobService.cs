using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cronos;

namespace pscommander 
{
    public class JobService
    {
        private PowerShellService _powerShellService;
        private readonly MenuService _menuService;
        private readonly List<CronJob> _cronJobs;
        private CancellationTokenSource _cancelSource;

        public JobService(PowerShellService powerShellService, MenuService menuService)
        {
            _powerShellService = powerShellService;
            _cronJobs = new List<CronJob>();
            _menuService = menuService;
        }
        public void ScheduleJobs(IEnumerable<Schedule> schedules)
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

            foreach(var schedule in schedules)
            {
                ScheduleJob(schedule);
            }
        }

        private void ScheduleJob(Schedule schedule)
        {
            var job = new CronJob(schedule, _powerShellService, _menuService);
            job.StartAsync(_cancelSource.Token).Wait();
            _cronJobs.Add(job);
        }
    }

    public class CronJob : IDisposable
    {
        private System.Timers.Timer _timer;
        private readonly CronExpression _expression;
        private readonly Schedule _schedule;
        private readonly PowerShellService _powerShellService;
        private readonly MenuService _menuService;


        public CronJob(Schedule schedule, PowerShellService powerShellService, MenuService menuService)
        {
            _schedule = schedule;
            _expression = CronExpression.Parse(schedule.Cron);
            _powerShellService = powerShellService;
            _menuService = menuService;
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleJob(cancellationToken);
        }

        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var next = _expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
                {
                    await ScheduleJob(cancellationToken);
                }
                _timer = new System.Timers.Timer(delay.TotalMilliseconds);
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
            }
            await Task.CompletedTask;
        }

        public virtual async Task DoWork(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            try 
            {
                _powerShellService.Execute(_schedule.Action);
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