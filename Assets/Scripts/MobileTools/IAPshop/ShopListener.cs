using System.Collections.Generic;
using MobileTools.SDK;
using MobileTools.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.SquadShooter;

namespace MobileTools.IAPshop
{
    public class ShopListener : MonoBehaviour
    {
        public Shop shop;
        public Button noAds;
        public List<ShopItem> items = new();
        public WeaponsController weapons;
        public ShopSave save;

        public ShopItem Find(string product)
            => items.Find(c => c.Id == product);

        void Start()
        {
            noAds.gameObject.SetActive(!Keys.IsNoAdsPurchased);
            noAds.onClick.AddListener(NoAds);
            SDKEvents.Instance.OnProductPurchase += Purchase;
            RefreshItems();
        }

        void NoAds()
        {
            SDKEvents.Instance.TryPurchaseNoAds();
        }

        void RefreshItems()
        {
            foreach (var item in items)
                item.SetLock(save.IsPurchased(item.Id));
        }

        void OnDisable()
        {
            if (!SDKEvents.Instance) return;
            SDKEvents.Instance.OnProductPurchase -= Purchase;
        }

        public void RemoveAds()
            => Keys.PurchaseNoAds();

        void Purchase(string id)
        {
            if (id == shop.noAdsID)
                RemoveAds();
            else
                GiveItem(id);
        }

        public CharactersDatabase characters;

        void GiveItem(string id)
        {
            var item = Find(id);
            BonusController.Instance.AddHp(item.hpBoostAmount);
            BonusController.Instance.AddHp(item.critBoostAmount);
            BonusController.Instance.AddHp(item.respawnBoostAmount);
            CurrenciesController.Add(CurrencyType.Coins, item.goldAmount);
            weapons.UnlockWeapon(item.weapon);

            var character = characters.Characters.Find(c => c.Type == item.skin);
            if (character.onlyShop) character.Purchase();

            save.Add(id);
            RefreshItems();
            // Bank.Instance.Add(ResourceEnum.Gold, gold);
            // if (item.expAmount > 0) EventManager.Instance.AddHeroExperience(Vector3.zero,item.expAmount);
        }
    }
}