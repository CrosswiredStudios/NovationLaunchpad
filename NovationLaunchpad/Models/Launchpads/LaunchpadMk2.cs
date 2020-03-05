using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Commons.Music.Midi;
using NovationLaunchpad.Interfaces;

namespace NovationLaunchpad.Models.Launchpads
{
    public enum LaunchpadMk2Color : byte
    {
        Off = 0,
        Black = 1,
        Grey = 2,
        White = 3,
        Salmon = 4,
        Red = 5,
        DarkRed = 6,
        DarkerRed = 7,
        Beige = 8,
        Orange = 9,
        Brown = 10,
        DarkBrown = 11,
        Offwhite = 12,
        Yellow = 13,
        DarkYellow = 14,
        DarkArmyGreen = 15,
        Teal = 16,
        ElectricGreen = 17,
        SkyBlue = 40,
        LightBlue = 41,
        DarkLightBlue = 42,
        DarkerLightBlue = 43,
        LightPurple = 44,
        Blue = 45,
        DarkerBlue = 46,
        DarkestBlue = 47,
        Pink = 53,
        Rose = 56,
        HotPink = 57,
        DarkHotPink = 58,
        DarkestHotPink = 59,
        DarkOrange = 60,
        Gold = 61,
        DarkBeige = 62,
        Green = 63
    }

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

        public override void Clear()
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 14, 0, 247 };
            _output.Send(command, 0, command.Length, 0);
        }

        #region PulseButton
        public void PulseButton(byte button, byte color)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 40, 0, button, color, 247 };
            _output.Send(command, 0, command.Length, 0);
        }

        public void PulseButton(int x, int y, byte color)
        {
            PulseButton(GetButtonId(x, y), color);
        }

        public void PulseButton(int x, int y, int color)
        {
            PulseButton(GetButtonId(x, y), (byte)color);
        }

        public void PulseButton(int x, int y, LaunchpadMk2Color color)
        {
            PulseButton(GetButtonId(x, y), (byte)color);
        }
        #endregion

        /// <summary>
        /// Writes all color data in the <see cref="GridBuffer"/> out to the Launchpad.
        /// </summary>
        /// <param name="clearBufferAfter">If true, this will clear the buffer after it's written to the Launchpad.</param>
        public void FlushGridBuffer(bool clearBufferAfter = true)
        {
            try
            {
                var commandBytes = new List<byte>();
                for (var y = 0; y < 8; y++)
                {
                    for (var x = 0; x < 8; x++)
                    {
                        //Test
                        var buttonId = (y + 1) * 10 + (x + 1);
                        Grid[x, y].Color = GridBuffer[x, y];
                        commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 11, (byte)buttonId, (byte)(GridBuffer[x, y].R / 4), (byte)(GridBuffer[x, y].G / 4), (byte)(GridBuffer[x, y].B / 4), 247 });

                        if (clearBufferAfter)
                            GridBuffer[x, y] = Color.Black;
                    }
                }
                _output.Send(commandBytes.ToArray(), 0, commandBytes.ToArray().Length, 0);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
