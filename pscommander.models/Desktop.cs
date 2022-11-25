using System.Collections.Generic;

namespace pscommander 
{
    public class Desktop
    {
        public IEnumerable<DesktopWidget> Widgets { get; set; } = new DesktopWidget[0];
    }
}