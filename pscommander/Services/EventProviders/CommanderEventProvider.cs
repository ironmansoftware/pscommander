using System;
using System.Collections.Generic;
using System.Linq;

namespace pscommander
{
    public class CommanderEventProvider : IEventProvider
    {
        public event EventHandler<EventProviderEvent> OnEvent;

        private int? _start;
        private int? _stop;
        private int? _error;

        public void Start()
        {
            if (_start.HasValue)
            {
                OnEvent?.Invoke(this, new EventProviderEvent(_start.Value));
            }
        }

        public void Stop()
        {
            if (_stop.HasValue)
            {
                OnEvent?.Invoke(this, new EventProviderEvent(_stop.Value));
            }
        }

        public void Error(string error)
        {
            if (_error.HasValue)
            {
                OnEvent?.Invoke(this, new EventProviderEvent(_error.Value));
            }
        }

        public void SetEvents(IEnumerable<CommanderEvent> events)
        {
            _start = events.FirstOrDefault(m => m.Category == "Commander" && m.Event == "Start")?.Id;
            _stop = events.FirstOrDefault(m => m.Category == "Commander" && m.Event == "Stop")?.Id;
            _error = events.FirstOrDefault(m => m.Category == "Commander" && m.Event == "Error")?.Id;
        }
    }
}