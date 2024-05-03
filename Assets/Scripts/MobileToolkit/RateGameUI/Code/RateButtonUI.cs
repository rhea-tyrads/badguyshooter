using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoreUI
{
    public class RateButtonUI : MonoBehaviour
    {
        public Button rateButton;
        public Image icon;
        [HideInInspector] public int id;

        void Awake()
        {
            rateButton.onClick.AddListener(Click);
        }

        public event Action<RateButtonUI> OnClick = delegate { };
        void Click() => OnClick(this);
    }
}