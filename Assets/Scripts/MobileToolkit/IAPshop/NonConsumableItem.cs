using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shops
{
    [Serializable]
    public class NonConsumableItem: MonoBehaviour
    {
        public string Id;
        public Button purchaseButton;
        public event Action<NonConsumableItem> OnTryPurchase = delegate { };

        void Awake()
            => purchaseButton.onClick.AddListener(TryPurchase);

        void TryPurchase()
            => OnTryPurchase(this);
    }
}