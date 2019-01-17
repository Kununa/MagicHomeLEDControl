using MagicHomeLEDControl;
using System;

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
            MagicHomeLED led = new MagicHomeLED("192.168.178.91", MagicHomeLED.Type.LD382v2);
            led.SetOn().Wait();
            Timer t = new Timer();
            t.setModeColor(155,0,0);
            t.setActive();
            led.SendTimers(new System.Collections.Generic.List<Timer> { t }).Wait();
            led.SetOff().Wait();
        }
    }
}
