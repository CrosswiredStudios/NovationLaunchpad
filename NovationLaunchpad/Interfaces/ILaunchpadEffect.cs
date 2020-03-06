using System;
namespace NovationLaunchpad.Interfaces
{
    public delegate void OnChangeFrequencyEvent(uint newFrequency);
    public delegate void OnCompleteEvent();

    public interface ILaunchpadEffect
    {
        event OnChangeFrequencyEvent OnChangeFrequency;
        event OnCompleteEvent OnComplete;

        string Name { get; }
        void Initiate(ILaunchpad launchpad);
        void Terminate();
        void Update();
        void UpdateFrequency(uint newFrequency);
    }
}
