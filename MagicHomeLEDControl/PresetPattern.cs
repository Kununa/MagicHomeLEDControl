using System.Collections.Generic;

namespace MagicHomeLEDControl
{
    public class PresetPattern
    {
        public enum Presets
        {
            sevenColorCrossFade = 37,
            redBreathing,
            greenBreathing,
            blueBreathing,
            yellowBreathing,
            cyanBreathing,
            purpleBreathing,
            whiteBreathing,
            redGreenBreathing,
            redBlueBreathing,
            greenBlueBreathing,
            sevenColorStrobeFlash,
            redStrobeFlash,
            greenStrobeFlash,
            blueStrobeFlash,
            yellowStrobeFlash,
            cyanStrobeFlash,
            purpleStrobeFlash,
            whiteStrobeFlash,
            sevenColorJumping
        };

        public static bool valid(byte pattern)
        {
            if (pattern < 0x25 || pattern > 0x38)
                return false;
            return true;
        }
    }
}
