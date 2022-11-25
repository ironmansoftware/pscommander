using System;
using System.Management.Automation;

namespace pscommander
{
    public class HotKey {
        public int Id { get; set; }
        public ScriptBlock Action { get; set; }
        public ModifierKeys ModifierKeys { get; set; }
        public Keys Keys { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is HotKey hk)
            {
                return hk.Id == Id;
            }
            return false;
        }
    }
}