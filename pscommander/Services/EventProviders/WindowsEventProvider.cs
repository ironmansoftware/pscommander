using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace pscommander
{
    public class WindowsEventProvider : IEventProvider
    {
        private readonly List<Task> _eventCheckTasks;
        private CancellationTokenSource _tokenSource;

        public WindowsEventProvider()
        {
            _tokenSource = new CancellationTokenSource();
            _eventCheckTasks = new List<Task>();
        }

        public event EventHandler<EventProviderEvent> OnEvent;

        public void SetEvents(IEnumerable<CommanderEvent> events)
        {
            _tokenSource.Cancel();
            Task.WaitAll(_eventCheckTasks.ToArray());
            _eventCheckTasks.Clear();
            _tokenSource = new CancellationTokenSource();

            foreach(var @event in events.Where(m => m.Category == "Windows"))
            {
                var eventType = @event.Properties["WmiEventType"];
                var eventFilter = @event.Properties["WmiEventFilter"];

                var task = Task.Run(async () => {
                    var query = new WqlEventQuery(eventType, new TimeSpan(0,0,1), eventFilter);                
                    var watcher = new ManagementEventWatcher();
                    watcher.Query = query;

                    while(!_tokenSource.Token.IsCancellationRequested)
                    {
                        await Task.Delay(1000);
                        if (_tokenSource.Token.IsCancellationRequested)
                        {
                            break;
                        }

                        try 
                        {
                            var e = watcher.WaitForNextEvent();
                            var targetInstance = (ManagementBaseObject)e["TargetInstance"];
                            OnEvent?.Invoke(this, new EventProviderEvent(@event.Id, targetInstance));
                        }
                        catch {}
                    }
                }, _tokenSource.Token);

                _eventCheckTasks.Add(task);
            }

             
        }
    }
}