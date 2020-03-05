using System;
using Commons.Music.Midi;
using NovationLaunchpad.Interfaces;
using NovationLaunchpad.Models;

namespace NovationLaunchpad
{
    public class Launchpad : LaunchpadBase
    {
        public ILaunchpad GetInstance(int index = 0)
        {
            throw new NotImplementedException();
        }
    }
}
