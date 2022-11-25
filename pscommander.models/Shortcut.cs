using System.Management.Automation;

namespace pscommander
{
    public class Shortcut 
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public ShortcutLocation Location { get; set; }
        public ScriptBlock Action { get; set; }
    }

    public enum ShortcutLocation
    {
        Desktop,
        StartMenu
    }
}