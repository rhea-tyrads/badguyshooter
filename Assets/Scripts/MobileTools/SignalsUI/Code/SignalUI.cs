using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileTools.SignalsUI.Code
{
    public class SignalUI : MonoBehaviour
    {
        public SignalType types;
        [Space(10)]
        public Image signal;
        [Space(20)]
        public List<SignalState> debugStates = new();
        public bool IsForceHide;
        public bool IsShow;
        public void Init()
        {
            foreach (SignalType type in Enum.GetValues(typeof(SignalType)))
            {
                if (type == SignalType.None) continue;
                if (!types.HasFlag(type)) continue;

                var state = new SignalState { type = type };
                debugStates.Add(state);
            }
        }

        public void Refresh(SignalState state)
        {
            if (!types.HasFlag(state.type)) return;

            var find = debugStates.Find(s => s.type == state.type);
            find.isActive = state.isActive;
            RefreshImage();
        }
        public void Refresh(List<SignalState> states)
        {
            foreach (var state in states)
                Refresh(state);

            RefreshImage();
        }
        public void RefreshImage()
        {
            var show = false;

            foreach (var item in debugStates)
            {
                if (!item.isActive) continue;
                show = true;
                break;
            }

            if (show)
                Show();
            else
                Hide();
        }

        public void Show()
        {
            IsShow = true;
            signal.enabled = IsForceHide ? false : true;
        }

        public void Hide()
        {
            IsShow = false;
            signal.enabled = false;
        }



        public void ForceHide(bool isForceHide)
        {
            IsForceHide = isForceHide;
            RefreshImage();
        }
    }
}
