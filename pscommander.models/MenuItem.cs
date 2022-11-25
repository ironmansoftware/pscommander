using System.Collections.Generic;
using System.Management.Automation;
using System.Windows.Input;

namespace pscommander
{
    public class MenuItem 
    {
        public string Text { get; set; }
        public ScriptBlock Action { get; set; }
        public IEnumerable<MenuItem> Children { get; set; }
        public ScriptBlock LoadChildren { get; set; }
        public object[] ArgumentList { get; set; }
    }
}