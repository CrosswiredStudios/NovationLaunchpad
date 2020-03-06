using System;
using System.Drawing;
using Commons.Music.Midi;

namespace NovationLaunchpad.Models.Launchpads
{
    public enum LaunchpadButtonState
    {
        Pressed,
        Released
    }

    public interface ILaunchpadButton
    {
        byte Channel { get; set; }
        Color Color { get; set; }
        byte Id { get; set; }
        bool IsPulsing { get; set; }
        LaunchpadButtonState State { get; set; }
        int X { get; set; }
        int Y { get; set; }
    }

    public class LaunchpadMk2Button : ILaunchpadButton
    {
        Color color;
        readonly IMidiOutput outPort;

        public byte Channel { get; set; }
        public Color Color
        {
            get => color;
            set
            {
                color = value;
            }
        }
        public byte Id { get; set; }
        public bool IsPulsing { get; set; }
        public LaunchpadButtonState State { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public LaunchpadMk2Button(byte channel, byte id, Color color)
        {
            this.color = color;
            Channel = channel;
            Id = id;
            IsPulsing = false;
            State = LaunchpadButtonState.Released;
        }

        public LaunchpadMk2Button(byte channel, int x, int y, Color color, IMidiOutput outPort)
        {
            this.color = color;
            this.outPort = outPort;
            Channel = channel;
            // LaunchpadMk2 numbering starts on the bottom left at 11. +1 as you go right, +10 as you go up
            Id = (byte)(((y + 1) * 10) + x + 1);
            IsPulsing = false;
            State = LaunchpadButtonState.Released;
            X = x;
            Y = y;
        }
    }
}
