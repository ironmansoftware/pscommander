using System.Management.Automation;

namespace pscommander 
{
    public class CustomProtocol
    {
        public string Protocol { get; set; }
        public ScriptBlock Action { get; set ;}
    }
}