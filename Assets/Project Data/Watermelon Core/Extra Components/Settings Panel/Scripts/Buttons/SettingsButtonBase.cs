﻿using UnityEngine;

namespace Watermelon
{
    public abstract class SettingsButtonBase : MonoBehaviour
    {
        int index;
        protected SettingsPanel settingsPanel;

        RectTransform rectTransform;
        public RectTransform RectTransform { get { return rectTransform; } }

        public void Init(int index, SettingsPanel settingsPanel)
        {
            this.index = index;
            this.settingsPanel = settingsPanel;

            this.rectTransform = GetComponent<RectTransform>();
        }

        public abstract bool IsActive();
        public abstract void OnClick();
    }
}

// -----------------
// Settings Panel v 0.3
// -----------------