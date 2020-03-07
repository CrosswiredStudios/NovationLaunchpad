using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;
using NovationLaunchpad.Interfaces;

// https://d2xhy469pqj8rc.cloudfront.net/sites/default/files/novation/downloads/10529/launchpad-mk2-programmers-reference-guide-v1-02.pdf

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
            : base(ports)
        {
            CreateGridButtons();

            // Listen for messages from the launchpad
            if(_input != null)
                _input.MessageReceived += OnMessageReceived;
        }

        public static async Task<LaunchpadMk2> GetInstance(int index = 0)
        {
            return new LaunchpadMk2(await GetMidiPorts(index));
        }

        /// <summary>
        /// Creates the objects that represent the grid buttons
        /// </summary>
        void CreateGridButtons()
        {
            Grid = new LaunchpadMk2Button[8, 8];
            for (var y = 0; y < 8; y++)
                for (var x = 0; x < 8; x++)
                {
                    Grid[x, y] = new LaunchpadMk2Button(0, x, y, Color.Black, _output);
                }
        }

        public override void Clear()
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 14, 0, 247 };
            _output.Send(command, 0, command.Length, 0);
        }

        private void OnMessageReceived(object sender, MidiReceivedEventArgs e)
        {
            try
            {
                switch (e.Data[0])
                {
                    // Grid Button Pressed
                    case 144:
                        var button = Grid[e.Data[1] % 10 - 1, e.Data[1] / 10 - 1];
                        button.State =
                            e.Data[2] == 0
                                ? LaunchpadButtonState.Released
                                : LaunchpadButtonState.Pressed;
                        ButtonStateChanged(button);
                        break;
                }
            }
            catch
            {

            }
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
        /// Sets the color of a grid button
        /// </summary>
        /// <param name="id">The id of the button</param>
        /// <param name="color">The color to set the button to.</param>
        public void SetGridButtonColor(int id, Color color)
        {
            try
            {
                // Logisticaly update the button
                var button = Grid[id % 10 - 1, id / 10 - 1];
                button.Color = color;
                
                // Create and send a command to set the light color
                var command = new byte[]
                {
                    240, 0, 32, 41, 2, 24, 11,
                    (byte) id,
                    (byte) (color.R / 4),
                    (byte) (color.G / 4),
                    (byte) (color.B / 4),
                    247
                };
                _output?.Send(command, 0, command.Length, 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to set the grid button color. {ex}");
            }
        }

        /// <summary>
        /// Sets the color of a grid button
        /// </summary>
        /// <param name="x">Grid x coordinate</param>
        /// <param name="y">Grid y coordinate</param>
        /// <param name="color">The color to set the button to</param>
        public void SetGridButtonColor(int x, int y, Color color)
        {
            // Convert x-y to id
            SetGridButtonColor((y + 1) * 10 + x + 1, color);
        }

        public void SetGridColor(Color[,] colors)
        {
            try
            {
                var commandBytes = new List<byte>();
                for (var y = 0; y < 8; y++)
                    for (var x = 0; x < 8; x++)
                    {
                        var color = colors[x, y];
                        var id = (y + 1) * 10 + x + 1;
                        // Logisticaly update the button
                        var button = Grid[id % 10 - 1, id / 10 - 1];
                        button.Color = color;
                        var command = new byte[] { 240, 0, 32, 41, 2, 24, 11, (byte)id, (byte)(color.R / 4), (byte)(color.G / 4), (byte)(color.B / 4), 247 };
                        _output?.Send(command, 0, command.Length, 0);
            }
                
            }
            catch (ObjectDisposedException ex)
            {
                Disconnected();
                Debug.WriteLine(ex);
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147023279)
                {
                    Disconnected();
                }
                Debug.WriteLine(ex);
            }
        }

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
