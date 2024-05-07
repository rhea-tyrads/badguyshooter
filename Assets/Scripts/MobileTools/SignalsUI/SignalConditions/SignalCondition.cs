using MobileTools.SignalsUI.Code;
using UnityEngine;

namespace MobileTools.SignalsUI.SignalConditions
{
    public abstract class SignalCondition : MonoBehaviour
    {
        protected SignalUI signal;


        void Awake()
        {
            signal = GetComponent<SignalUI>();
        }

        protected virtual void ForceHide(bool forceHide)
            => signal.ForceHide(forceHide);
    }
}
