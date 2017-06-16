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

        public static byte[] addCheckSum(byte[] input)
        {
            byte sum = 0;
            foreach (byte b in input)
                sum = (byte)(sum + b);
            byte[] output = input.Concat(new byte[] { sum }).ToArray();
            return output;
        }
    }
}
