using System;
using NovationLaunchpad.Models.Launchpads;

namespace NovationLaunchpad.Interfaces
{
    public interface ILaunchpad
    {
        event OnButtonStateChangedEvent OnButtonStateChanged;
        /// <summary>
        /// Clears all lights on the 
        /// </summary>
        void Clear();
    }
}
