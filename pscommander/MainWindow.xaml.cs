using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Windows;

namespace pscommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PowerShellService powerShellService;
        private readonly ConfigService configService;
        private readonly JobService jobService;
        private readonly MenuService menuService;
        private readonly HotKeyService hotKeyService;
        private readonly DataService dataService;
        private readonly ShortcutService shortcutService;
        private readonly FileAssociationService fileAssociationService;
        private readonly NamedPipeClient namedPipeClient;
        private readonly NamedPipeService namedPipeService;
        private readonly CommandService commandService;
        private readonly ContextMenuService contextMenuService;
        private readonly EventService eventService;
        private readonly CommanderEventProvider commanderEventProvider;
        private readonly WindowsEventProvider windowsEventProvider;
        private readonly CustomProtocolService protocolService;
        private readonly DesktopService desktopService;
        private readonly DataSourceService dataSourceService;
        private readonly BlinkService blinkService;

        public MainWindow()
        {
            InitializeComponent();

            powerShellService = new PowerShellService();
            dataService = new DataService();
            commanderEventProvider = new CommanderEventProvider();
            windowsEventProvider = new WindowsEventProvider();

            menuService = new MenuService(powerShellService, commanderEventProvider);
            dataSourceService = new DataSourceService(powerShellService, menuService);
            desktopService = new DesktopService(powerShellService, dataSourceService);
            jobService = new JobService(powerShellService, menuService);
            hotKeyService = new HotKeyService(powerShellService, menuService);

            shortcutService = new ShortcutService(dataService, powerShellService, menuService);
            fileAssociationService = new FileAssociationService(dataService, powerShellService, menuService);
            contextMenuService = new ContextMenuService(dataService, powerShellService, menuService);
            protocolService = new CustomProtocolService(dataService, powerShellService, menuService);

            commandService = new CommandService(fileAssociationService, shortcutService, contextMenuService, protocolService);
            namedPipeClient = new NamedPipeClient();
            namedPipeService = new NamedPipeService(commandService, menuService);

            eventService = new EventService(menuService, powerShellService, new IEventProvider[] {
                commanderEventProvider,
                windowsEventProvider
            });

            blinkService = new BlinkService();

            configService = new ConfigService(
                powerShellService,
                menuService,
                jobService,
                hotKeyService,
                fileAssociationService,
                shortcutService,
                contextMenuService,
                eventService,
                protocolService,
                desktopService,
                dataSourceService,
                blinkService);

            powerShellService.Initialize(new Dictionary<string, object> {
                { "DataService", dataService },
                { "MenuService", menuService },
                { "HotKeyService", hotKeyService },
                { "JobService", jobService },
                { "ShortcutService", shortcutService },
                { "FileAssociationService", fileAssociationService },
                { "DesktopService", desktopService },
            });

            Visibility = Visibility.Hidden;
            configService.LoadAsync().Wait();

            commanderEventProvider.Start();
            App.Current.Exit += (s, e) => commanderEventProvider.Stop();

            if (!configService.Configuration.Settings.DisableUpdateCheck)
            {
                var updateCheck = Task.Run(() =>
                {
                    try
                    {
                        var currentVersion = powerShellService.ExecuteNewRunspace<Version>("(Get-Module PSCommander -ListAvailable).Version | Sort-Object -Descending").First();
                        var galleryVersion = powerShellService.ExecuteNewRunspace<Version>("[Version](Find-Module PSCommander).Version").First();

                        if (currentVersion < galleryVersion)
                        {
                            menuService.ShowInfo($"An update is available for PSCommander ({galleryVersion}). Run Update-Module PSCommander to upgrade.");
                        }
                    }
                    catch { }
                });
            }

        }
    }
}
