using System.Collections.Generic;
using MobileTools.Utilities;
using UnityEngine;

namespace MobileTools.IAPshop
{
    public class GoldShopListener : MonoBehaviour
    {
        public GoldShop shop;
        public List<ConsumableItem> consumables = new();
        ConsumableItem Find(string product) => consumables.Find(c => c.Id == product);

        void Start()
        {
            //     EventManager.Instance.OnProductPurchase += Purchase;
        }

        void OnDisable()
        {
            //   if (!EventManager.Instance) return;
            //   EventManager.Instance.OnProductPurchase -= Purchase;
        }

        public void RemoveAds()
            => Keys.PurchaseNoAds();

        void Purchase(string id)
        {
            if (id == shop.noAdsID)
                RemoveAds();
            else
                AddGold(id);
        }

        void AddGold(string product, int quantity = 1)
        {
            var item = Find(product);
            for (var i = 0; i < quantity; i++)
            {
                var gold = item.goldAmount;
                // Bank.Instance.Add(ResourceEnum.Gold, gold);
                // if (item.expAmount > 0) EventManager.Instance.AddHeroExperience(Vector3.zero,item.expAmount);
            }
        }
    }
}