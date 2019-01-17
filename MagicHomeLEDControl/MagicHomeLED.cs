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

        private async Task<byte[]> Receive(byte[] input)
        {
            if (input == null)
                return null;
            byte[] data = Utils.AddCheckSum(input);

            using (TcpClient client = new TcpClient(ipaddr, port))
            {
                using (NetworkStream stream = client.GetStream())
                {
                    await stream.WriteAsync(data, 0, data.Length);
                    data = new byte[14];
                    System.Threading.Thread.Sleep(1000);
                    client.Client.ReceiveTimeout = 1000;
                    await stream.ReadAsync(data, 0, data.Length);

                }
            }
            return data;
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

    }
}
