using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace pscommander
{
    public class CustomProtocolService
    {
        private readonly DataService _dataService;
        private readonly PowerShellService _powerShellService;
        private readonly List<CustomProtocol> _protocols;
        private readonly MenuService _menuService;

        public CustomProtocolService(DataService dataService, PowerShellService powerShellService, MenuService menuService)
        {
            _dataService = dataService;
            _powerShellService = powerShellService;
            _protocols = new List<CustomProtocol>();
            _menuService = menuService;
        }

        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        public void ExecuteProtocol(string protocolName, string url)
        {
            var protocol = _protocols.FirstOrDefault(m => m.Protocol.Equals(protocolName, StringComparison.OrdinalIgnoreCase));
            if (protocol == null) return;

            try 
            {
                var arg = url.Split("://").LastOrDefault().TrimEnd('/');
                _powerShellService.Execute(protocol.Action, arg);
            }
            catch (Exception ex)
            {
                _menuService.ShowError(ex.Message);
            }
        }

        public void SetProtocols(IEnumerable<CustomProtocol> customProtocols)
        {
            var existingProtocols = _dataService.CustomProtocols.FindAll();
            foreach(var existingProtocol in existingProtocols)
            {
                RemoveProtocol(existingProtocol);
            }

            _dataService.CustomProtocols.DeleteAll();
            _protocols.Clear();

            foreach(var protocol in customProtocols)
            {
                AddProtocol(protocol);
            }
        }

        private void RemoveProtocol(CustomProtocol protocol)
        {
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{protocol.Protocol}", "", "");
            SHChangeNotify(0x08000000, 0x2000, IntPtr.Zero, IntPtr.Zero);
        }

        private void AddProtocol(CustomProtocol protocol)
        {
            var exePath = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe");

            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{protocol.Protocol}", "", $"URL:{protocol.Protocol}");
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{protocol.Protocol}", "URL Protocol", "");
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{protocol.Protocol}\\shell\\open\\command", "",  $"{exePath} --protocol \"{protocol.Protocol}\" --protocolArg \"%1\"");
            SHChangeNotify(0x08000000, 0x2000, IntPtr.Zero, IntPtr.Zero);

            _dataService.CustomProtocols.Insert(protocol);
            _protocols.Add(protocol);
        }
    }
}