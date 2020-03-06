using System;
using NovationLaunchpad.Interfaces;

namespace NovationLaunchpad.Models.Effects
{
    public abstract class LaunchpadEffect : ILaunchpadEffect, IDisposable
    {
        public event OnChangeFrequencyEvent OnChangeFrequency;
        public event OnCompleteEvent OnComplete;

        public virtual string Name => "Launchpad Effect";

        public LaunchpadEffect()
        {

        }

        public virtual void Dispose()
        {

        }

        public virtual void Initiate(ILaunchpad launchpad)
        {

        }

        public virtual void Terminate()
        {
            OnComplete?.Invoke();
        }

        public virtual void Update()
        {

        }

        public virtual void UpdateFrequency(uint newFrequency)
        {
            OnChangeFrequency?.Invoke(newFrequency);
        }
    }
}
