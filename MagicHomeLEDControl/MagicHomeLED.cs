using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MagicHomeLEDControl
{
    public class MagicHomeLED
    {

        public enum Type
        {
            LW12,
            LD316,
            LD316A,
            LD382,
            LD382v2
        }

        private string ipaddr;
        private int port;
        private byte red;
        private byte green;
        private byte blue;
        public byte WarmWhite { get; internal set; }
        public bool IsOn { get; internal set; }
        public Type type;

        public MagicHomeLED(string ipaddr, Type type, int port = 5577)
        {
            this.ipaddr = ipaddr;
            this.port = port;
            IsOn = false;
            red = 0;
            green = 0;
            blue = 0;
            WarmWhite = 0;
            this.type = type;
            GetState().Wait();
        }

        private async Task<bool> Send(byte[] input)
        {
            if (input == null)
                return false;
            byte[] data = Utils.AddCheckSum(input);

            using (NetworkStream stream = new TcpClient(ipaddr, port).GetStream())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            return true;
        }

        private async Task<byte[]> Receive(byte[] input, byte length = 14)
        {
            if (input == null)
                return null;
            byte[] data = Utils.AddCheckSum(input);
            byte[] response;

            using (TcpClient client = new TcpClient(ipaddr, port) { ReceiveTimeout = 1000 })
            {
                using (NetworkStream stream = client.GetStream())
                {
                    await stream.WriteAsync(data, 0, data.Length);
                    response = new byte[length];
                    int read = 0;
                    while (read < length)
                        read += await stream.ReadAsync(response, read, response.Length - read);

                }
            }
            return response;
        }

        public async Task SetRGB(Color c)
        {
            await SetRGB(c.R, c.G, c.B);
        }

        public async Task SetRGB(byte r, byte g, byte b)
        {
            byte[] data = null;
            if (type == Type.LD382v2)
                data = new byte[] { 0x31, r, g, b, 0x00, 0x00, 0x0f };
            if (await Send(data))
            {
                red = r;
                green = g;
                blue = b;
                WarmWhite = 0;
            }
            if (!IsOn)
                await SetOn();
        }

        public async Task SetRGBW(byte r, byte g, byte b, byte w)
        {
            byte[] data = null;
            if (type == Type.LD382v2)
                data = new byte[] { 0x31, r, g, b, w, 0x00, 0x0f };
            if (await Send(data))
            {
                red = r;
                green = g;
                blue = b;
                WarmWhite = w;
            }
            if (!IsOn)
                await SetOn();
        }

        public async Task SetWarmWhite(byte ww)
        {
            byte[] data = null;
            if (type == Type.LD382v2)
                data = new byte[] { 0x31, 0x00, 0x00, 0x00, ww, 0x00, 0x0f };
            if (await Send(data))
            {
                WarmWhite = ww;
                red = 0;
                green = 0;
                blue = 0;
            }
            if (!IsOn)
                await SetOn();
        }

        public async Task SetOn()
        {
            byte[] data = null;
            if (type == Type.LD382v2)
                data = new byte[] { 0x71, 0x23, 0x0f };
            if (await Send(data))
                IsOn = true;
        }

        public async Task SetOff()
        {
            byte[] data = null;
            if (type == Type.LD382v2)
                data = new byte[] { 0x71, 0x24, 0x0f };
            if (await Send(data))
                IsOn = false;
        }

        public async Task GetState()
        {
            byte[] data = null;
            if (type == Type.LD382v2)
                data = new byte[] { 0x81, 0x8a, 0x8b };
            byte[] ans = await Receive(data);
            if (ans == null || ans[0] != 0x81)
                return;
            if (ans[2] == 0x23)
                IsOn = true;
            else if (ans[2] == 0x24)
                IsOn = false;
            red = ans[6];
            green = ans[7];
            blue = ans[8];
            WarmWhite = ans[9];

            byte delay = ans[5];


            if (new byte[] { 0x61, 0x62 }.Contains(ans[3]) && ans[9] == 0)
            {
                red = ans[6];
                green = ans[7];
                blue = ans[8];
                WarmWhite = ans[9];
            }
            else if (PresetPattern.valid(ans[3]))
            {
                //is pattern
            }
            else if (ans[3] == 0x60)
            {
                //custom pattern
            }
        }

        public async Task SetPresetPattern(PresetPattern.Presets pattern, byte speed)
        {
            byte delay = Utils.SpeedToDelay(speed);
            byte[] data = new byte[] { 0x61, (byte)pattern, delay, 0x0f };
            await Send(data);
        }

        public async Task SetClockToSystem()
        {
            await SetClock(DateTime.Now);
        }

        public async Task SetClock(DateTime dt)
        {
            List<int> data = new List<int> { 0x10, 0x14 };
            data.Add(dt.Year - 2000);
            data.Add(dt.Month);
            data.Add(dt.Day);
            data.Add(dt.Hour);
            data.Add(dt.Minute);
            data.Add(dt.Second);
            data.Add((dt.DayOfWeek == 0) ? 7 : (int)dt.DayOfWeek);
            data.Add(0x00);
            data.Add(0x0f);
            await Send(data.Select(x => (byte)x).ToArray());
        }

        public async Task<DateTime> GetClock()
        {
            byte[] data = new byte[] { 0x11, 0x1a, 0x1b, 0x0f };
            byte[] rx = await Receive(data, 12);

            int year = rx[3] + 2000;
            int month = rx[4];
            int day = rx[5];
            int hour = rx[6];
            int minute = rx[7];
            int second = rx[8];

            DateTime dt = new DateTime(year, month, day, hour, minute, second);
            return dt;
        }

        public async Task<List<Timer>> GetTimers()
        {
            byte[] data = new byte[] { 0x22, 0x2a, 0x2b, 0x0f };
            byte[] rx = await Receive(data, 88);


            // utils.dump_data(rx)
            int start = 2;
            List<Timer> timer_list = new List<Timer>();
            //pass in the 14-byte timer structs
            for (int i = 0; i < 6; i++)
            {
                byte[] timer_bytes = new byte[14];
                Array.Copy(rx, start, timer_bytes, 0, 14);
                Timer timer = new Timer(timer_bytes);
                timer_list.Add(timer);
                start += 14;
            }

            return timer_list;
        }

        public async Task SendTimers(List<Timer> timer_list)
        {
            // remove inactive or expired timers from list
            foreach (var t in timer_list)
                if (!t.isActive() || t.isExpired())
                    timer_list.Remove(t);


            // truncate if more than 6
            if (timer_list.Count > 6)
            {
                Console.WriteLine("too many timers, truncating list");
                timer_list = timer_list.GetRange(0, 6);
            }

            // pad list to 6 with inactive timers
            if (timer_list.Count != 6)
                for (int i = 0; i < 6 - timer_list.Count; i++)

                    timer_list.Add(new Timer());



            byte msg_start = 0x21;
            byte[] msg_end = new byte[] { 0x00, 0xf0 };
            List<byte> msg = new List<byte>();

            // build message
            msg.Add(msg_start);
            foreach (var t in timer_list)
                msg.AddRange(t.toBytes());

            msg.AddRange(msg_end);

            await Send(msg.ToArray());
        }


    }
}
