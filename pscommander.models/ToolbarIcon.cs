using System.Collections.Generic;
using System.Management.Automation;

namespace pscommander 
{
    public class ToolbarIcon
    {
        public string Text { get; set; }
        public string Icon { get; set; }
        public ScriptBlock LoadItems { get; set; }
        public IEnumerable<MenuItem> MenuItems { get; set; }
        public bool HideExit { get; set; }
        public bool HideConfig { get; set; }
    }
}