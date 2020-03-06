using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons.Music.Midi;
using NovationLaunchpad.Interfaces;
using SkiaSharp;

namespace NovationLaunchpad.Models.Launchpads
{
    public abstract class LaunchpadBase : ILaunchpad
    {
        public Dictionary<ILaunchpadEffect, Timer> Effects { get; }
        protected IMidiInput _input;
        protected IMidiOutput _output;

        public LaunchpadMk2Button[,] Grid { get; }
        public Color[,] GridBuffer { get; private set; }

        public LaunchpadBase()
        {
            Grid = new LaunchpadMk2Button[8, 8];
            GridBuffer = new Color[8, 8];
            Effects = new Dictionary<ILaunchpadEffect, Timer>();
        }

        protected static byte GetButtonId(int x, int y)
        {
            return (byte)((y+1) * 10 + (x+1));
        }

        protected static async Task<(IMidiInput, IMidiOutput)> GetMidiPorts(int index = 0)
        {
            var access = MidiAccessManager.Default;
            var launchpad = access.Outputs.Where(o => o.Name.ToLower().Contains("launchpad")).ToArray()[index];
            return (null, await access.OpenOutputAsync(launchpad.Id));
        }

        public abstract void Clear();

        /// <summary>
        /// Add an effect to the launchpad
        /// </summary>
        /// <param name="effect">The effect to add to the launchpad.</param>
        /// <param name="updateFrequency">The frequency at which the update method will be called.</param>
        public void RegisterEffect(ILaunchpadEffect effect, TimeSpan updateFrequency)
        {
            try
            {
                // Create an update timer at the specified frequency
                Effects.Add(effect, new Timer(state => effect.Update(), null, 0, (int)updateFrequency.TotalMilliseconds));
                effect.OnChangeFrequency += (newFrequency) =>
                {
                    Effects[effect].Change(0, newFrequency);
                };
                effect.OnComplete += () =>
                {
                    Effects.Remove(effect);
                };
                // Initiate the effect (provide all buttons and button changed event
                effect.Initiate(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Add an effect to the launchpad
        /// </summary>
        /// <param name="effect">The effect to add to the launchpad.</param>
        /// <param name="updateFrequency">The frequency in milliseconds at which the update method will be called.</param>
        public void RegisterEffect(ILaunchpadEffect effect, uint updateFrequency)
        {
            RegisterEffect(effect, TimeSpan.FromMilliseconds(updateFrequency));
        }
    }
}
