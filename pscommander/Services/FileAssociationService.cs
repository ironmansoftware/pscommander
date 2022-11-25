using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace pscommander
{
    public class FileAssociationService
    {
        private readonly DataService _dataService;
        private readonly PowerShellService _powerShellService;
        private readonly List<FileAssociation> _fileAssociations;
        private readonly MenuService _menuService;

        public FileAssociationService(DataService dataService, PowerShellService powerShellService, MenuService menuService)
        {
            _dataService = dataService;
            _powerShellService = powerShellService;
            _fileAssociations = new List<FileAssociation>();
            _menuService = menuService;
        }

        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        public void ExecuteAssociation(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var fileAssociation = _fileAssociations.FirstOrDefault(m => m.Extension.Equals(fileInfo.Extension, StringComparison.OrdinalIgnoreCase));
            if (fileAssociation == null) return;

            try 
            {
                _powerShellService.Execute(fileAssociation.Action, filePath);
            }
            catch (Exception ex)
            {
                _menuService.ShowError(ex.Message);
            }
        }

        public void SetAssociations(IEnumerable<FileAssociation> fileAssociations)
        {
            var existingAssociations = _dataService.FileAssociations.FindAll();
            foreach(var existingAssociation in existingAssociations)
            {
                RemoveAssociate(existingAssociation);
            }

            _dataService.FileAssociations.DeleteAll();
            _fileAssociations.Clear();

            foreach(var association in fileAssociations)
            {
                AddAssociate(association);
            }
        }

        private void RemoveAssociate(FileAssociation association)
        {
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\PSCommander\\shell\\open\\command", "",  "");
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{association.Extension}", "", "");
            SHChangeNotify(0x08000000, 0x2000, IntPtr.Zero, IntPtr.Zero);
        }

        private void AddAssociate(FileAssociation association)
        {
            var exePath = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe");

            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\PSCommander\\shell\\open\\command", "",  $"{exePath} --filePath \"%1\"");
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{association.Extension}", "", $"PSCommander");
            SHChangeNotify(0x08000000, 0x2000, IntPtr.Zero, IntPtr.Zero);

            _dataService.FileAssociations.Insert(association);
            _fileAssociations.Add(association);
        }
    }
}