using System;
using MobileTools.SignalsUI.Code;
using UnityEngine;

namespace MobileTools.SignalsUI.SignalListeners
{
    public abstract class SignalListener: MonoBehaviour
    {
        public SignalType types;
        public SignalState state;
        public event Action<SignalState> OnChange = delegate { };

        public void Init()
        {
            state.type = types;
            state.isActive = false;
            OnInit();
        }
        protected abstract void OnInit();


        protected void Refresh()=> OnChange(null);
        protected void Active()
        {
            state.isActive = true;
            OnChange(state);
        }
        protected void Passive()
        {
            state.isActive = false;
            OnChange(state);
        }
    }
}