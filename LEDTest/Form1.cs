using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MagicHomeLEDControl;

namespace LEDTest
{
    public partial class Form1 : Form
    {
        MagicHomeLED led;

        public Form1()
        {
            InitializeComponent();
            led = new MagicHomeLED("192.168.178.91");
            led.setWW(100);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                led.setRGB(colorDialog1.Color.R, colorDialog1.Color.G, colorDialog1.Color.B);
            }
        }
    }
}
