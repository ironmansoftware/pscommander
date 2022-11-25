using System.Collections.Generic;
using System.Management.Automation;

namespace pscommander 
{
    public class Schedule
    {
        public string Cron { get; set; }
        public ScriptBlock Action { get; set; }
    }
}