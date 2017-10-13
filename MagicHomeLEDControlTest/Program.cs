using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MagicHomeLEDControl;

namespace MagicHomeLEDControlTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MagicHomeLED led = new MagicHomeLED("192.168.178.108", MagicHomeLED.Type.LD382v2);
        }
    }
}
