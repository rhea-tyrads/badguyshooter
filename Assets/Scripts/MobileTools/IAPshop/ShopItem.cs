using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using Watermelon.SquadShooter;

namespace MobileTools.IAPshop
{
    public class ShopItem : MonoBehaviour
    {
        #region Inspector

        public string Id;
        public ShopPurchaseType purchaseType;
        public ShopItemType itemType;
        
        [HideIf(nameof(IsWeapon))]
        public BonusPackUI bonuses;

        [ShowIf(nameof(ShowWeapon))] public WeaponType weapon;
        [ShowIf(nameof(IsBundle))] public CharacterSkinType skin;
        [ShowIf(nameof(IsBooster))] public int hpBoostAmount;
        [ShowIf(nameof(IsBooster))] public int critBoostAmount;
        [ShowIf(nameof(IsBooster))] public int respawnBoostAmount;
        [ShowIf(nameof(IsBooster))] public int goldAmount;


        public Button purchaseButton;
        bool ShowWeapon => IsWeapon || IsBundle;
        bool IsWeapon => itemType == ShopItemType.Weapon;
        bool IsBundle => itemType == ShopItemType.Bundle;
        bool IsBooster => itemType == ShopItemType.BoosterPack || IsBundle;
        public event Action<ShopItem> OnTryPurchase = delegate { };

        #endregion

        public GameObject purchasedContainer;
        
        void Awake()
            => purchaseButton.onClick.AddListener(TryPurchase);

        public void SetLock(bool isPurchased)
        {
            purchasedContainer.SetActive(isPurchased);
        }
        
        public void Set(PackData data)
        {
            if (!bonuses) return;
            data.goldAmount = goldAmount;
            data.critAmount = critBoostAmount;
            data.hpAmount = hpBoostAmount;
            data.respawnAmount = respawnBoostAmount;
            bonuses.Init(data);
        }

        void TryPurchase()
            => OnTryPurchase(this);
    }

    public enum ShopItemType
    {
        Weapon,
        Bundle,
        BoosterPack
    }

    public enum ShopPurchaseType
    {
        OneTimePurchase,
        ManyTimePurchase
    }

    public enum CharacterSkinType
    {
        None,
        Chicken,
        Ninja
    }
}