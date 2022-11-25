using System;
using System.Collections.Generic;
using System.Linq;

namespace pscommander
{
    public class EventService
    {
        private readonly Dictionary<int, CommanderEvent> _events;
        private readonly MenuService _menuService;
        private readonly IEnumerable<IEventProvider> _eventProviders;
        private readonly PowerShellService _powerShellService;

        public EventService(MenuService menuService, PowerShellService powerShellService, IEnumerable<IEventProvider> eventProviders)
        {
            _menuService = menuService;
            _powerShellService = powerShellService;
            _eventProviders = eventProviders;
            _events = new Dictionary<int, CommanderEvent>();

            foreach(var eventProvider in _eventProviders)
            {
                eventProvider.OnEvent += OnEvent;
            }
        }

        public void SetEvents(IEnumerable<CommanderEvent> events)
        {
            _events.Clear();

            foreach(var item in events)
            {
                _events.Add(item.Id, item);
            }

            foreach(var item in _eventProviders)
            {
                item.SetEvents(events);
            }
        }

        private void OnEvent(object sender, EventProviderEvent arguments)
        {
            if (!_events.ContainsKey(arguments.Id))
            {
                return;
            }

            var registeredEvent = _events[arguments.Id];

            try 
            {
                _powerShellService.Execute(registeredEvent.Action, arguments.Arguments);
            }
            catch (Exception ex)
            {
                _menuService.ShowError(ex.Message);
            }
        }
    }
}