using System;
using System.Linq;

namespace MagicHomeLEDControl
{
    class BuiltInTimer
    {
        public enum Mode
        {
            sunrise = 0xA1,
            sunset
        }

        public static bool valid(byte byte_value)
        {
            return byte_value == (byte)Mode.sunrise || byte_value == (byte)Mode.sunset;
        }

        public static string valtostr(byte pattern)
        {
            foreach (var i in Enum.GetValues(typeof(Mode)).Cast<Mode>())
            {
                if ((byte)i == pattern)
                    return nameof(i);
            }

            return "";
        }
    }

    public class Timer
    {
        private const byte Mo = 0x02;
        private const byte Tu = 0x04;
        private const byte We = 0x08;
        private const byte Th = 0x10;
        private const byte Fr = 0x20;
        private const byte Sa = 0x40;
        private const byte Su = 0x80;
        private const byte Everyday = Mo | Tu | We | Th | Fr | Sa | Su;
        private const byte Weekdays = Mo | Tu | We | Th | Fr;
        private const byte Weekend = Sa | Su;

        int repeat_mask;
        int year;
        int month;
        int day;
        int hour;
        int minute;
        int second;
        bool active;
        string mode;
        byte pattern_code;
        bool turn_on;
        byte red;
        byte green;
        byte blue;
        byte warmth_level;
        byte delay;
        byte duration;
        byte brightness_start;
        byte brightness_end;

        /*@staticmethod
        public dayMaskToStr(mask) :
        for key, value in LedTimer.__dict__.items({
            if type(value) is int and value == mask:
                return key
        return None*/

        public Timer(byte[] bytes = null)
        {
            if (bytes != null)
            {
                fromBytes(bytes);
                return;
            }

            DateTime dt = DateTime.Now.AddHours(1);
            this.setTime(dt.Hour, dt.Minute);
            this.setDate(dt.Year, dt.Month, dt.Day);
            this.setModeTurnOff();
            this.setActive(false);
        }

        public void setActive(bool active = true)
        {
            this.active = active;
        }

        public bool isActive()
        {
            return this.active;
        }

        public bool isExpired()
        {
            //if no repeat mask and datetime is in past, return true
            if (this.repeat_mask != 0)
                return false;
            else if (this.year != 0 && this.month != 0 && this.day != 0)
            {
                DateTime dt = new DateTime(this.year, this.month, this.day, this.hour, this.minute, 0);
                if (dt < DateTime.Now)
                    return true;
            }
            return false;
        }

        public void setTime(int hour, int minute)
        {
            this.hour = hour;
            this.minute = minute;
        }

        public void setDate(int year, int month, int day)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.repeat_mask = 0;
        }

        public void setRepeatMask(byte repeat_mask)
        {
            this.year = 0;
            this.month = 0;
            this.day = 0;
            this.repeat_mask = repeat_mask;
        }

        public void setModeDefault()
        {
            this.mode = "default";
            this.pattern_code = 0;
            this.turn_on = true;
            this.red = 0;
            this.green = 0;
            this.blue = 0;
            this.warmth_level = 0;
        }

        public void setModePresetPattern(byte pattern, byte speed)
        {
            this.mode = "preset";
            this.warmth_level = 0;
            this.pattern_code = pattern;
            this.delay = Utils.SpeedToDelay(speed);
            this.turn_on = true;
        }

        public void setModeColor(byte r, byte g, byte b)
        {
            this.mode = "color";
            this.warmth_level = 0;
            this.red = r;
            this.green = g;
            this.blue = b;
            this.pattern_code = 0x61;
            this.turn_on = true;
        }

        public void setModeWarmWhite(byte level)
        {
            this.mode = "ww";
            this.warmth_level = Utils.percentToByte(level);
            this.pattern_code = 0x61;
            this.red = 0;
            this.green = 0;
            this.blue = 0;
            this.turn_on = true;
        }

        public void setModeSunrise(byte startBrightness, byte endBrightness, byte duration)
        {
            this.mode = "sunrise";
            this.turn_on = true;
            this.pattern_code = (byte)BuiltInTimer.Mode.sunrise;
            this.brightness_start = Utils.percentToByte(startBrightness);
            this.brightness_end = Utils.percentToByte(endBrightness);
            this.warmth_level = Utils.percentToByte(endBrightness);
            this.duration = duration;
        }

        public void setModeSunset(byte startBrightness, byte endBrightness, byte duration)
        {
            this.mode = "sunrise";
            this.turn_on = true;
            this.pattern_code = (byte)BuiltInTimer.Mode.sunset;
            this.brightness_start = Utils.percentToByte(startBrightness);
            this.brightness_end = Utils.percentToByte(endBrightness);
            this.warmth_level = Utils.percentToByte(endBrightness);
            this.duration = duration;
        }


        public void setModeTurnOff()
        {
            this.mode = "off";
            this.turn_on = false;
            this.pattern_code = 0;
        }

        /*
        timer are in six 14-byte structs
            f0 0f 08 10 10 15 00 00 25 1f 00 00 00 f0 0f
             0  1  2  3  4  5  6  7  8  9 10 11 12 13 14
            0: f0 when active entry/ 0f when not active
            1: (0f=15) year when no repeat, else 0
            2:  month when no repeat, else 0
            3:  dayofmonth when no repeat, else 0
            4: hour
            5: min
            6: 0
            7: repeat mask, Mo=0x2,Tu=0x04, We 0x8, Th=0x10 Fr=0x20, Sa=0x40, Su=0x80
            8:  61 for solid color or warm, or preset pattern code
            9:  r (or delay for preset pattern)
            10: g
            11: b
            12: warm white level
            13: 0f = off, f0 = on?
        */
        public void fromBytes(byte[] bytes)
        {
            // Utils.dump_bytes(bytes)
            this.red = 0;
            this.green = 0;
            this.blue = 0;
            if (bytes[0] == 0xf0)
                this.active = true;
            else
                this.active = false;
            this.year = bytes[1] + 2000;
            this.month = bytes[2];
            this.day = bytes[3];
            this.hour = bytes[4];
            this.minute = bytes[5];
            this.repeat_mask = bytes[7];
            this.pattern_code = bytes[8];




            if (this.pattern_code == 0x00)
                this.mode = "default";
            else if (this.pattern_code == 0x61)
            {
                this.mode = "color";
                this.red = bytes[9];
                this.green = bytes[10];
                this.blue = bytes[11];
            }
            else if (BuiltInTimer.valid(this.pattern_code))
            {
                this.mode = BuiltInTimer.valtostr(this.pattern_code);
                this.duration = bytes[9]; //same byte as red
                this.brightness_start = bytes[10]; // same byte as green
                this.brightness_end = bytes[11]; // same byte as blue
            }
            else if (PresetPattern.valid(this.pattern_code))
            {
                this.mode = "preset";
                this.delay = bytes[9];  //same byte as red
            }
            else
                this.mode = "unknown";


            this.warmth_level = bytes[12];
            if (this.warmth_level != 0)
                this.mode = "ww";


            if (bytes[13] == 0xf0)
                this.turn_on = true;
            else
            {
                this.turn_on = false;
                this.mode = "off";
            }
        }


        public byte[] toBytes()
        {
            byte[] bytes = new byte[14];
            if (!this.active)
            {
                bytes[0] = 0x0f;
                // quit since all other zeros is good
                return bytes;
            }


            bytes[0] = 0xf0;


            if (this.year >= 2000)
                bytes[1] = (byte)(this.year - 2000);
            else
                bytes[1] = (byte)this.year;
            bytes[2] = (byte)this.month;
            bytes[3] = (byte)this.day;
            bytes[4] = (byte)this.hour;
            bytes[5] = (byte)this.minute;
            // what is 6?
            bytes[7] = (byte)this.repeat_mask;


            if (!this.turn_on)
            {
                bytes[13] = 0x0f;
                return bytes;
            }
            bytes[13] = 0xf0;


            bytes[8] = this.pattern_code;
            if (PresetPattern.valid(this.pattern_code))
            {
                bytes[9] = this.delay;
                bytes[10] = 0;
                bytes[11] = 0;
            }
            else if (BuiltInTimer.valid(this.pattern_code))
            {
                bytes[9] = this.duration;
                bytes[10] = this.brightness_start;
                bytes[11] = this.brightness_end;
            }
            else
            {
                bytes[9] = this.red;
                bytes[10] = this.green;
                bytes[11] = this.blue;
            }
            bytes[12] = this.warmth_level;

            return bytes;
        }
    }
}
