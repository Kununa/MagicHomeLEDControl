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
        private byte ww;
        private bool isOn;
        public Type type;

        public MagicHomeLED(string ipaddr, Type type, int port = 5577)
        {
            this.ipaddr = ipaddr;
            this.port = port;
            isOn = false;
            red = 0;
            green = 0;
            blue = 0;
            ww = 0;
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
                ww = 0;
            }
            if (!isOn)
                await SetOn();
        }

        public async Task SetWW(byte ww)
        {
            byte[] data = null;
            if (type == Type.LD382v2)
                data = new byte[] { 0x31, 0x00, 0x00, 0x00, ww, 0x00, 0x0f };
            if (await Send(data))
            {
                this.ww = ww;
                red = 0;
                green = 0;
                blue = 0;
            }
            if (!isOn)
                await SetOn();
        }

        public byte getWW()
        {
            return ww;
        }

        public async Task SetOn()
        {
            byte[] data = null;
            if (type == Type.LD382v2)
                data = new byte[] { 0x71, 0x23, 0x0f };
            if (await Send(data))
                isOn = true;
        }

        public async Task SetOff()
        {
            byte[] data = null;
            if (type == Type.LD382v2)
                data = new byte[] { 0x71, 0x24, 0x0f };
            if (await Send(data))
                isOn = false;
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
                isOn = true;
            else if (ans[2] == 0x24)
                isOn = false;
            red = ans[6];
            green = ans[7];
            blue = ans[8];
            ww = ans[9];

            byte delay = ans[5];


            if (new byte[] { 0x61, 0x62 }.Contains(ans[3]) && ans[9] == 0)
            {
                red = ans[6];
                green = ans[7];
                blue = ans[8];
                ww = ans[9];
            }
            else if (PresetPattern.valid(ans[3]))
            {
                string pat = PresetPattern.valtostr(ans[3]);
            }
            else if (ans[3] == 0x60)
            {
                //custom pattern
            }
        }

        public bool IsOn()
        {
            return isOn;
        }

    }
}
