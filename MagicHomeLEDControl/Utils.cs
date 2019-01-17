using System.Linq;

namespace MagicHomeLEDControl
{
    class Utils
    {
        private const byte max_delay = 0x1f;

        public static byte[] AddCheckSum(byte[] input)
        {
            byte sum = 0;
            foreach (byte b in input)
                sum = (byte)(sum + b);
            byte[] output = input.Concat(new byte[] { sum }).ToArray();
            return output;
        }

        public static byte SpeedToDelay(byte speed)
        {
            if (speed > 100)
                speed = 100;
            if (speed < 0)
                speed = 0;
            byte inv_speed = (byte)(100 - speed);
            double delay = (speed / 100.0) * 30.0;
            delay++;
            return (byte)delay;
        }
    }
}
