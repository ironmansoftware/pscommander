using System.Collections.Generic;

namespace pscommander
{
    public class BlinkService
    {
        private IEnumerable<Blink> _blinks;

        public void SetBlinks(IEnumerable<Blink> blinks)
        {
            _blinks = blinks;
        }

        public BlinkService()
        {

        }
    }
}