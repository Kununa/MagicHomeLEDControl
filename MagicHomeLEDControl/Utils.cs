using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MagicHomeLEDControl
{
    class Utils
    {

        private const byte max_delay = 0x1f;

        public static int delayToSpeed(byte delay)
        {
            // speed is 0-100, delay is 1-31
            // 1st translate delay to 0-30
            delay = (byte)(delay - 1);
            if (delay > max_delay - 1)
                delay = max_delay - 1;

            if (delay < 0)
                delay = 0;

            int inv_speed = (int)((delay * 100) / (max_delay - 1));
            int speed = 100 - inv_speed;
            return speed;
        }
    }
}
