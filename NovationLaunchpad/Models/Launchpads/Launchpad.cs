using System;
using Commons.Music.Midi;
using NovationLaunchpad.Interfaces;
using NovationLaunchpad.Models;

namespace NovationLaunchpad.Models.Launchpads
{
    public class Launchpad : LaunchpadBase
    {
        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public ILaunchpad GetInstance(int index = 0)
        {
            throw new NotImplementedException();
        }
    }
}
