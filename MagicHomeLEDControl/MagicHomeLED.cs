using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace MagicHomeLEDControl
{
    public class MagicHomeLED
    {

        private string ipaddr;
        private int port;
        private byte red;
        private byte green;
        private byte blue;
        private byte ww;
        private bool isOn;
        private string state_str;

        public MagicHomeLED(string ipaddr, int port = 5577)
        {
            this.ipaddr = ipaddr;
            this.port = port;
            isOn = false;
            red = 0;
            green = 0;
            blue = 0;
            ww = 0;
            state_str = "";
        }

        private bool send(byte[] input)
        {
            byte sum = 0;
            foreach (byte b in input)
                sum = (byte)(sum + b);
            byte[] data = input.Concat(new byte[] { sum }).ToArray();

            TcpClient client = new TcpClient(ipaddr, port);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            stream.Close();
            client.Close();
            return true;
        }

        private byte[] receive(byte[] input)
        {
            byte sum = 0;
            foreach (byte b in input)
                sum = (byte)(sum + b);
            byte[] data = input.Concat(new byte[] { sum }).ToArray();

            TcpClient client = new TcpClient(ipaddr, port);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            data = new byte[14];
            stream.Read(data, 0, 14);

            stream.Close();
            client.Close();
            return data;
        }

        public void setRGB(byte r, byte g, byte b)
        {
            byte[] data = { 0x31, r, g, b, 0x00, 0x00, 0x0f };
            if (send(data))
            {
                red = r;
                green = g;
                blue = b;
                ww = 0;
            }
            if (!isOn)
                setOn();
        }

        public void setWW(byte ww)
        {
            byte[] data = { 0x31, 0x00, 0x00, 0x00, ww, 0x00, 0x0f };
            if (send(data))
            {
                this.ww = ww;
                red = 0;
                green = 0;
                blue = 0;
            }
            if (!isOn)
                setOn();
        }

        public void setOn()
        {
            byte[] data = { 0x71, 0x23, 0x0f };
            if (send(data))
                isOn = true;
        }

        public void setOff()
        {
            byte[] data = { 0x71, 0x24, 0x0f };
            if (send(data))
                isOn = false;
        }

        public void getState()
        {
            string power_str = "Unknown power state";
            string mode_str = "";
            byte[] ans = receive(new byte[] { 0x81, 0x8a, 0x8b });
            if (ans[0] != 0x81)
                return;
            if (ans[2] == 0x23)
            {
                isOn = true;
                power_str = "ON ";
            }
            else if (ans[2] == 0x24)
            {
                isOn = false;
                power_str = "OFF";
            }

            byte pattern = ans[3];
            byte ww_level = ans[9];
            string mode = Utils.determineMode(ww_level, pattern);
            byte delay = ans[5];

            int speed = Utils.delayToSpeed(delay);


            if (mode == "color")
            {
                red = ans[6];
                green = ans[7];
                blue = ans[8];
                string color_str = Utils.color_tuple_to_string(red, green, blue);
                mode_str = "Color: " + color_str;
            }
            else if (mode == "ww")
                mode_str = "Warm White: {}" + Utils.byteToPercent(ww_level) + "%";
            else if (mode == "preset")
            {
                string pat = PresetPattern.valtostr(pattern);
                mode_str = "Pattern: " + pat + " (Speed " + speed + "%)";
            }
            else if (mode == "custom")
                mode_str = "Custom pattern (Speed " + speed + "%)";

            else
                mode_str = String.Format("Unknown mode 0x{0:x}", pattern);

            if (pattern == 0x62)
                mode_str += " (tmp)";

            state_str = power_str + " [" + mode_str + "]";
        }


    }
}
