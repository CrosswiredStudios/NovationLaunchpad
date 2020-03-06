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
    public delegate void OnButtonStateChangedEvent(ILaunchpadButton button);
    public delegate void OnDisconnectedEvent();

    public abstract class LaunchpadBase : ILaunchpad
    {
        public event OnButtonStateChangedEvent OnButtonStateChanged;
        public event OnDisconnectedEvent OnDisconnected;

        protected IMidiInput _input;
        protected IMidiOutput _output;

        public Dictionary<ILaunchpadEffect, Timer> Effects { get; }
        public LaunchpadMk2Button[,] Grid { get; protected set; }
        public Color[,] GridBuffer { get; private set; }

        public LaunchpadBase((IMidiInput, IMidiOutput) ports)
        {
            _input = ports.Item1;
            _output = ports.Item2;
            
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
            var launchpadInput = access.Inputs.Where(o => o.Name.ToLower().Contains("launchpad")).ToArray()[index];
            var launchpadOutput = access.Outputs.Where(o => o.Name.ToLower().Contains("launchpad")).ToArray()[index];
            return (await access.OpenInputAsync(launchpadInput.Id), await access.OpenOutputAsync(launchpadOutput.Id));
        }

        public void ButtonStateChanged(ILaunchpadButton button)
        {
            OnButtonStateChanged?.Invoke(button);
        }

        public abstract void Clear();

        /// <summary>
        /// Should be called when the system cannot detect the launchpad.
        /// </summary>
        public void Disconnected()
        {
            OnDisconnected?.Invoke();
        }

        #region Effects
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

        /// <summary>
        /// Remove an effect from the launchpad.
        /// </summary>
        /// <param name="effect">The effect to attempt to remove</param>
        public void UnregisterEffect(ILaunchpadEffect effect)
        {
            try
            {
                // Dispose of the timer
                Effects[effect]?.Dispose();
                // Dispose of the effect
                effect?.Dispose();
                // Remove it from the dictionary
                Effects.Remove(effect);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Remove all effect from the launchpad.
        /// </summary>
        public void UnregisterAllEffects()
        {
            try
            {
                foreach (var effect in Effects)
                {
                    // Dispose of the timer
                    effect.Value.Dispose();
                    // Dispose of the effect
                    effect.Key.Dispose();
                }
                Effects.Clear();
            }
            catch
            {
            }
        }
        #endregion
    }
}
