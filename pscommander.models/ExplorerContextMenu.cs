using System.Management.Automation;

namespace pscommander
{
    public class ExplorerContextMenu 
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ContextMenuLocation Location { get; set; }
        public ScriptBlock Action { get; set; }
        public string Extension { get; set; }
        public string Icon { get; set; }
        public int IconIndex { get; set; }
        public bool Extended { get; set; }
        public ContextMenuPosition Position { get; set; }
    }

    public enum ContextMenuPosition
    {
        None,
        Top,
        Bottom
    }

    public enum ContextMenuLocation
    {
        FolderLeftPanel,
        FolderRightPanel,
        File
    }
}