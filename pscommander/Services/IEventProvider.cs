using System;
using System.Collections.Generic;

namespace pscommander
{
    public interface IEventProvider 
    {
        event EventHandler<EventProviderEvent> OnEvent;
        void SetEvents(IEnumerable<CommanderEvent> events);
    }

    public class EventProviderEvent : EventArgs 
    {
        public int Id { get; }
        public object[] Arguments { get; }

        public EventProviderEvent(int id)
        {
            Id = id;
            Arguments = new object[0];
        }

        public EventProviderEvent(int id, params object[] arguments)
        {
            Id = id;
            Arguments = arguments;
        }
    }
}