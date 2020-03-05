using System;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;
using NovationLaunchpad.Interfaces;

namespace NovationLaunchpad.Models
{
    public abstract class LaunchpadBase : ILaunchpad
    {
        protected IMidiInput _input;
        protected IMidiOutput _output;

        public LaunchpadBase()
        {
        }

        protected static async Task<(IMidiInput, IMidiOutput)> GetMidiPorts(int index = 0)
        {
            var access = MidiAccessManager.Default;
            var launchpad = access.Outputs.Where(o => o.Name.ToLower().Contains("launchpad")).ToArray()[index];
            return (null, await access.OpenOutputAsync(launchpad.Id));
        }

        public void Clear()
        {
        }
    }
}
