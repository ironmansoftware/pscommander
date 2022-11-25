using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace pscommander
{
    public class ContextMenuService
    {
        private readonly DataService _dataService;
        private readonly PowerShellService _powerShellService;
        private readonly MenuService _menuService;
        private readonly List<ExplorerContextMenu> _menuItems;

        public ContextMenuService(DataService dataService, PowerShellService powerShellService, MenuService menuService)
        {
            _dataService = dataService;
            _powerShellService = powerShellService;
            _menuService = menuService;
            _menuItems = new List<ExplorerContextMenu>();
        }

        public void SetContextMenuItems(IEnumerable<ExplorerContextMenu> contextMenuItems)
        {
            var existing = _dataService.ExplorerContextMenus.FindAll();
            var exePath = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe");

            foreach(var item in existing)
            {
                string location = item.Extension;
                if (item.Location == ContextMenuLocation.FolderLeftPanel)
                {
                    location = "directory\\Background";
                }

                if (item.Location == ContextMenuLocation.FolderRightPanel)
                {
                    location = "directory";
                }
                
                using(var key = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{location}\\shell", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    try 
                    {
                        key.DeleteSubKeyTree($"PSCommander{item.Id}");
                    }
                    catch {}
                }
            }

            _dataService.ExplorerContextMenus.DeleteAll();
            _menuItems.Clear();

            foreach(var item in contextMenuItems)
            {
                string location = item.Extension;
                if (item.Location == ContextMenuLocation.FolderLeftPanel)
                {
                    location = "directory\\Background";
                }

                if (item.Location == ContextMenuLocation.FolderLeftPanel)
                {
                    location = "directory";
                }
                
                using(var key = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{location}\\shell\\PSCommander{item.Id}"))
                {
                    key.SetValue(null, item.Text);

                    if (item.Extended)
                    {
                        key.SetValue("Extended", string.Empty);
                    }

                    if (!string.IsNullOrWhiteSpace(item.Icon))
                    {
                        key.SetValue("icon", $"{item.Icon},{item.IconIndex}");
                    }

                    if (item.Position != ContextMenuPosition.None)
                    {
                        key.SetValue("Position", item.Position.ToString());
                    }

                    using(var subkey = key.CreateSubKey("command"))
                    {
                        subkey.SetValue(null, $"{exePath} --context {item.Id} --contextPath \"%1\" ");
                    }
                }

                _dataService.ExplorerContextMenus.Insert(item);
                _menuItems.Add(item);
            }
        }

        public void ExecuteMenuItem(int id, string filePath)
        {
            var item = _menuItems.FirstOrDefault(m => m.Id == id);
            if (item == null) return;

            try 
            {
                _powerShellService.Execute(item.Action, filePath);
            }
            catch (Exception ex)
            {
                _menuService.ShowError(ex.Message);
            }
        }
    }
}