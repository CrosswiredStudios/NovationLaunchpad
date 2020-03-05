using System;
using NovationLaunchpad.Models;

namespace NovationLaunchpad.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var launchpad = LaunchpadMk2.GetInstance().Result;
            launchpad.PulseButton(11, 5);
        }
    }
}
