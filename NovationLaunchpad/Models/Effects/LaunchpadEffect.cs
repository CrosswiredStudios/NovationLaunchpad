using System;
using NovationLaunchpad.Interfaces;

namespace NovationLaunchpad.Models.Effects
{
    public abstract class LaunchpadEffect : ILaunchpadEffect
    {
        public event OnChangeFrequencyEvent OnChangeFrequency;
        public event OnCompleteEvent OnComplete;

        public virtual string Name => "Launchpad Effect";

        public LaunchpadEffect()
        {

        }

        public virtual void Initiate(ILaunchpad launchpad)
        {

        }

        public virtual void Terminate()
        {

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
