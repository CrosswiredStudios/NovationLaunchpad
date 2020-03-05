using System;
using System.Threading.Tasks;
using Commons.Music.Midi;
using NovationLaunchpad.Interfaces;

namespace NovationLaunchpad.Models
{
    public class LaunchpadMk2 : LaunchpadBase
    {
        LaunchpadMk2((IMidiInput, IMidiOutput) ports)
        {
            _input = ports.Item1;
            _output = ports.Item2;
        }

        public static async Task<LaunchpadMk2> GetInstance(int index = 0)
        {
            return new LaunchpadMk2(await GetMidiPorts(index));
        }

        public void PulseButton(byte button, byte color)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 40, 0, button, color, 247 };
            _output.Send(command, 0, command.Length, 0);
        }
    }
}
