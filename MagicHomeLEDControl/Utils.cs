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

        public static string determineMode(byte ww_level, byte pattern_code)
        {
            string mode = "unknown";
            if (new byte[] { 0x61, 0x62 }.Contains(pattern_code))
            {

                if (ww_level != 0)
                    mode = "ww";
                else
                    mode = "color";
            }
            else if (pattern_code == 0x60)
                mode = "custom";
            else if (PresetPattern.valid(pattern_code))
                mode = "preset";

            return mode;
        }

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

        public static string color_tuple_to_string(byte red, byte green, byte blue)
        {
            // try to convert to an english name
            /*try:
			return webcolors.rgb_to_name(rgb)
            except Exception as e:
			#print e
			pass
    
            return str(rgb)*/
            string hex = "#" + red + green + blue;
            Color color = System.Drawing.ColorTranslator.FromHtml(hex);
            return "";
        }

        public static int byteToPercent(byte b)
        {
            return (int)((b * 100) / 255);
        }
    }
}
