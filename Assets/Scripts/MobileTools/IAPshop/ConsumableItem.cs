using System;
using UnityEngine;
using UnityEngine.UI;

namespace MobileTools.IAPshop
{
    public class ConsumableItem : MonoBehaviour
    {
        public string Id;
        public int goldAmount;
        public float expAmount;
        public Button purchaseButton;
        public event Action<ConsumableItem> OnTryPurchase = delegate { };

        void Awake()
            => purchaseButton.onClick.AddListener(TryPurchase);

        void TryPurchase()
            => OnTryPurchase(this);
    }
}