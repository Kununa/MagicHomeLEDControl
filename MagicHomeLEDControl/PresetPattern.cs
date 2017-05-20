using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicHomeLEDControl
{
    class PresetPattern
    {
        public static Dictionary<string, byte> presets = new Dictionary<string, byte>{
            {"seven_color_cross_fade", 0x25 },
            {"red_gradual_change", 0x26},
            {"green_gradual_change", 0x27},
            {"blue_gradual_change", 0x28},
            {"yellow_gradual_change", 0x29},
            {"cyan_gradual_change", 0x2a},
            {"purple_gradual_change", 0x2b},
            {"white_gradual_change", 0x2c},
            {"red_green_cross_fade", 0x2d},
            {"red_blue_cross_fade", 0x2e},
            {"green_blue_cross_fade", 0x2f},
            {"seven_color_strobe_flash", 0x30},
            {"red_strobe_flash", 0x31},
            {"green_strobe_flash", 0x32},
            {"blue_stobe_flash", 0x33},
            {"yellow_strobe_flash", 0x34},
            {"cyan_strobe_flash", 0x35},
            {"purple_strobe_flash", 0x36},
            {"white_strobe_flash", 0x37},
            {"seven_color_jumping", 0x38}};

        public static bool valid(byte pattern)
        {
            if (pattern < 0x25 || pattern > 0x38)
                return false;
            return true;
        }

        public static string valtostr(byte pattern)
        {
            foreach (KeyValuePair<string, byte> entry in presets)
                if (entry.Value == pattern)
                    return entry.Key.Replace("_", " ");

            return "";
        }
    }
}
