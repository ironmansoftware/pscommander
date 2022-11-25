using System.Management.Automation;

namespace pscommander 
{
    public class FileAssociation
    {
        public int Id { get; set; }
        public string Extension { get; set; }
        public ScriptBlock Action { get; set; }
    }
}