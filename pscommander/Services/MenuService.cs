using System.Windows.Controls;
using WPFMenuItem = System.Windows.Controls.MenuItem;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Management.Automation;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace pscommander
{
    public class MenuService
    {
        private readonly TaskbarIcon _taskbarIcon;
        private readonly PowerShellService _powerShellService;
        private readonly CommanderEventProvider _commanderEventProvider;

        public MenuService(PowerShellService powerShellService, CommanderEventProvider commanderEventProvider)
        {
            _taskbarIcon = new TaskbarIcon();
            _powerShellService = powerShellService;
            _commanderEventProvider = commanderEventProvider;
        }

        public void UpdateToolbar(ToolbarIcon icon)
        {
            var assemblyLocation = Assembly.GetEntryAssembly().Location;
            var fileInfo = new FileInfo(assemblyLocation);

            _taskbarIcon.Dispatcher.Invoke(() => {

                if (string.IsNullOrEmpty(icon.Text))
                {
                    _taskbarIcon.ToolTipText = "PSCommander";
                }
                else 
                {
                    _taskbarIcon.ToolTipText = icon.Text;
                }

                if (string.IsNullOrWhiteSpace(icon.Icon))
                {
                    _taskbarIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/pscommander;component/Resources/icon.ico"));
                }
                else 
                {
                    _taskbarIcon.Icon = Icon.ExtractAssociatedIcon(icon.Icon);
                }

                _taskbarIcon.ContextMenu = new ContextMenu();
                _taskbarIcon.PreviewTrayContextMenuOpen += (s, e) => {
                    _taskbarIcon.ContextMenu.Items.Clear();

                    if (icon.MenuItems != null)
                    {
                        foreach(var menuItem in icon.MenuItems)
                        {
                            AddMenuItem(_taskbarIcon.ContextMenu.Items, menuItem);
                        }
                    }   

                    if (icon.LoadItems != null)
                    {
                        try 
                        {
                            var menuItems = _powerShellService.Execute<MenuItem>(icon.LoadItems);
                            foreach(var menuItem in menuItems)
                            {
                                AddMenuItem(_taskbarIcon.ContextMenu.Items, menuItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowError(ex.Message);
                        }
                    }

                    if (!icon.HideConfig)
                    {
                        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var path = Path.Combine(documents, "PSCommander", "config.ps1");

                        AddMenuItem(_taskbarIcon.ContextMenu.Items, new MenuItem {
                            Text = "Edit Config",
                            Action = ScriptBlock.Create($". '{Path.Combine(fileInfo.DirectoryName, "PSScriptPad.exe")}' -c '{path}'")
                        });
                    }

                    if (!icon.HideExit)
                    {
                        AddMenuItem(_taskbarIcon.ContextMenu.Items, new MenuItem {
                            Text = "Exit",
                            Action = ScriptBlock.Create($"[System.Environment]::Exit(0)")
                        });
                    }
                };
                _taskbarIcon.MenuActivation = PopupActivationMode.RightClick;
            });
        }

        private void AddMenuItem(ItemCollection items, MenuItem item)
        {
            var wpfMenuItem = new WPFMenuItem();
            wpfMenuItem.Header = item.Text;
            wpfMenuItem.SubmenuOpened += (s, e) => {
                if (item.LoadChildren != null)
                {
                    wpfMenuItem.Items.Clear();
                    if (item.Children != null)
                    {
                        foreach(var menuItem in item.Children)
                        {
                            AddMenuItem(wpfMenuItem.Items, menuItem);
                        }
                    }      

                    try 
                    {
                        var menuItems = _powerShellService.Execute<MenuItem>(item.LoadChildren);
                        foreach(var menuItem in menuItems)
                        {
                            AddMenuItem(wpfMenuItem.Items, menuItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                }
            };
            wpfMenuItem.Click += (s, e) => { 
                e.Handled = true;
                if (item.Action == null) return;

                try 
                {
                    _powerShellService.Execute(item.Action, item.ArgumentList);
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            };
            
            items.Add(wpfMenuItem);

            if (item.Children != null)
            {
                foreach(var child in item.Children)
                {
                    AddMenuItem(wpfMenuItem.Items, child);
                }
            }
            else if (item.LoadChildren != null)
            {
                AddMenuItem(wpfMenuItem.Items, new MenuItem { Text = "Default" });
            }
        }

        public void ShowError(string error)
        {
            _commanderEventProvider.Error(error);
            _taskbarIcon.ShowBalloonTip("Error", error, BalloonIcon.Error);
        }

        public void ShowInfo(string info)
        {
            _taskbarIcon.ShowBalloonTip("Info", info, BalloonIcon.Info);
        }
    }
}