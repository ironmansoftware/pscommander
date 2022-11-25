using System.Collections.Generic;
using System.Management.Automation;

namespace pscommander 
{
    public class CommanderEvent 
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Event { get ; set; }
        public ScriptBlock Action { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}