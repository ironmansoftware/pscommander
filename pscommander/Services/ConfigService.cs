using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace pscommander
{
    public class ConfigService
    {
        private readonly PowerShellService _powerShellService;
        private readonly MenuService _menuService;
        private readonly JobService _jobService;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly HotKeyService _hotKeyService;
        private readonly FileAssociationService _fileAssociationService;
        private readonly ShortcutService _shortcutService;
        private readonly ContextMenuService _contextMenuService;
        private readonly EventService _eventService;
        private readonly CustomProtocolService _protocolService;
        private readonly DesktopService _desktopService;
        private readonly DataSourceService _dataSourceService;
        private readonly BlinkService _blinkService;
        private readonly object locker = new object();
        public static string ConfigFilePath { get; set; }
        public Configuration Configuration { get; set; }

        public ConfigService(
            PowerShellService powerShellService,
            MenuService menuService,
            JobService jobService,
            HotKeyService hotKeyService,
            FileAssociationService fileAssociationService,
            ShortcutService shortcutService,
            ContextMenuService contextMenuService,
            EventService eventService,
            CustomProtocolService protocolService,
            DesktopService desktopService,
            DataSourceService dataSourceService,
            BlinkService blinkService)
        {
            _powerShellService = powerShellService;
            _menuService = menuService;
            _jobService = jobService;
            _hotKeyService = hotKeyService;
            _protocolService = protocolService;
            _desktopService = desktopService;
            _dataSourceService = dataSourceService;
            _blinkService = blinkService;

            // while(!System.Diagnostics.Debugger.IsAttached)
            // {
            //     Thread.Sleep(100);
            // }           

            if (!Directory.Exists(GetFolder()))
            {
                Directory.CreateDirectory(GetFolder());
            }

            _fileSystemWatcher = new FileSystemWatcher(GetFolder());
            _fileSystemWatcher.Created += (s, e) => { Debounce(LoadAsync); };
            _fileSystemWatcher.Changed += (s, e) => { Debounce(LoadAsync); };
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileAssociationService = fileAssociationService;
            _shortcutService = shortcutService;
            _contextMenuService = contextMenuService;
            _eventService = eventService;
        }

        private string GetFolder()
        {
            if (!string.IsNullOrWhiteSpace(ConfigFilePath))
            {
                var fileInfo = new FileInfo(ConfigFilePath.Trim('\''));
                return fileInfo.DirectoryName;
            }

            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, "PSCommander");
        }

        public async Task LoadAsync()
        {
            Configuration = new Configuration();

            await Task.CompletedTask;
            var configPath = Path.Combine(GetFolder(), "config.ps1");
            if (!File.Exists(configPath))
            {
                return;
            }

            var scriptBlock = ScriptBlock.Create($"& '{configPath}'");

            try
            {
                var objects = _powerShellService.Execute<object>(scriptBlock);

                Configuration.ToolbarIcon = objects.OfType<ToolbarIcon>().FirstOrDefault();
                Configuration.Settings = objects.OfType<Settings>().FirstOrDefault();
                Configuration.HotKeys = objects.OfType<HotKey>().ToArray();
                Configuration.Schedules = objects.OfType<Schedule>().ToArray();
                Configuration.FileAssociations = objects.OfType<FileAssociation>().ToArray();
                Configuration.Shortcuts = objects.OfType<Shortcut>().ToArray();
                Configuration.ContextMenus = objects.OfType<ExplorerContextMenu>().ToArray();
                Configuration.Events = objects.OfType<CommanderEvent>().ToArray();
                Configuration.Protocols = objects.OfType<CustomProtocol>().ToArray();
                Configuration.DataSources = objects.OfType<DataSource>().ToArray();
                Configuration.Blinks = objects.OfType<Blink>().ToArray();
                Configuration.Desktop = objects.OfType<Desktop>().FirstOrDefault();

                if (Configuration.ToolbarIcon == null)
                {
                    Configuration.ToolbarIcon = new ToolbarIcon();
                }

                if (Configuration.Settings == null)
                {
                    Configuration.Settings = new Settings();
                }

                if (Configuration.Desktop == null)
                {
                    Configuration.Desktop = new Desktop();
                }

                Configuration.Desktop.Widgets = Configuration.Desktop.Widgets.Concat(objects.OfType<DesktopWidget>().ToArray()).ToArray();

                _menuService.UpdateToolbar(Configuration.ToolbarIcon);
                _hotKeyService.SetHotKeys(Configuration.HotKeys);
                _jobService.ScheduleJobs(Configuration.Schedules);
                _fileAssociationService.SetAssociations(Configuration.FileAssociations);
                _shortcutService.SetShortcuts(Configuration.Shortcuts);
                _contextMenuService.SetContextMenuItems(Configuration.ContextMenus);
                _eventService.SetEvents(Configuration.Events);
                _protocolService.SetProtocols(Configuration.Protocols);
                _dataSourceService.SetDataSources(Configuration.DataSources);
                _desktopService.SetDesktop(Configuration.Desktop);
                _blinkService.SetBlinks(Configuration.Blinks);
            }
            catch (Exception ex)
            {
                _menuService.ShowError(ex.Message);
                _menuService.UpdateToolbar(new ToolbarIcon());
            }

            if (Configuration.Settings == null)
            {
                Configuration.Settings = new Settings();
            }
        }

        public static void Debounce(Func<Task> action, int milliseconds = 1000)
        {
            CancellationTokenSource lastCToken = null;
            lastCToken?.Cancel();
            try
            {
                lastCToken?.Dispose();
            }
            catch { }

            var tokenSrc = lastCToken = new CancellationTokenSource();

            Task.Delay(milliseconds).ContinueWith(async task => { await action(); }, tokenSrc.Token);
        }
    }
}