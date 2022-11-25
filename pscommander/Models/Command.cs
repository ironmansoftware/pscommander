using System.Collections.Generic;

namespace pscommander
{
    public class Command 
    {
        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}