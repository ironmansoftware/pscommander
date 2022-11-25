using System.Collections.Generic;

namespace pscommander
{
    public class Configuration
    {
        public IEnumerable<HotKey> HotKeys { get; set; } = new List<HotKey>();
        public IEnumerable<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ToolbarIcon ToolbarIcon { get; set; } = new ToolbarIcon();
        public Settings Settings { get; set; } = new Settings();
        public IEnumerable<FileAssociation> FileAssociations { get; set; } = new List<FileAssociation>();
        public IEnumerable<Shortcut> Shortcuts { get; set; } = new List<Shortcut>();
        public IEnumerable<ExplorerContextMenu> ContextMenus { get; set; } = new List<ExplorerContextMenu>();
        public IEnumerable<CommanderEvent> Events { get; set; } = new List<CommanderEvent>();
        public IEnumerable<CustomProtocol> Protocols { get; set; } = new List<CustomProtocol>();
        public Desktop Desktop { get; set; } = new Desktop();
        public IEnumerable<DataSource> DataSources { get; set; } = new List<DataSource>();
        public IEnumerable<Blink> Blinks { get; set; } = new List<Blink>();
    }
}
