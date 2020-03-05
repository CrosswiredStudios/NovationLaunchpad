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
        protected Dictionary<ILaunchpadEffect, Timer> EffectsTimers { get; }
        protected IMidiInput _input;
        protected IMidiOutput _output;

        public LaunchpadMk2Button[,] Grid { get; }
        public Color[,] GridBuffer { get; private set; }

        public LaunchpadBase()
        {
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
        /// <param name="effect"></param>
        /// <param name="updateFrequency"></param>
        public void RegisterEffect(ILaunchpadEffect effect, TimeSpan updateFrequency)
        {
            try
            {
                // Register any observables being used
                CompositeDisposable effectDisposables = new CompositeDisposable();

                effect.OnChangeFrequency += (frewq) => OnChangeFrequencyEvent(effect, frewq);
                // If this effect needs the ability to change its frequency
                if (effect.OnChangeFrequency != null)
                {
                    // Subscribe to the event to change the frequency and add it to this effects disposables
                    effectDisposables.Add(
                        effect
                        .WhenChangeUpdateFrequency
                        .Subscribe(newFrequency =>
                        {
                            // Change the frequency for this effect
                            OnChangeEffectUpdateFrequency(effect, newFrequency);
                        }));
                }

                // If this effect will notify us it needs to be unregistered
                if (effect.WhenComplete != null)
                {
                    effectDisposables.Add(
                        effect
                        .WhenComplete
                        .Subscribe(_ =>
                        {
                            // Unregister the effect and destroy its disposables
                            UnregisterEffect(effect);
                        }));
                }

                EffectsDisposables.Add(effect, effectDisposables);

                // Create an update timer at the specified frequency
                EffectsTimers.Add(effect, new Timer(state => effect.Update(), null, 0, (int)updateFrequency.TotalMilliseconds));

                // Initiate the effect (provide all buttons and button changed event
                effect.Initiate(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
