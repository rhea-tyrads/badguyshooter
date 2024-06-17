//using NaughtyAttributes;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class WeaponData
    {

        [SerializeField] string name;
        [SerializeField] [TextArea] string description;
        public bool availableOnlyInShop;
        public string Name => name;
        public string Description => description;

        [SerializeField] WeaponType type;
        public WeaponType Type => type;

        [SerializeField] UpgradeType upgradeType;
        public UpgradeType UpgradeType => upgradeType;

        [SerializeField] Rarity rarity;
        public Rarity Rarity => rarity;

    //    [ShowAssetPreview()]
    [SerializeField] Sprite icon;
    public Sprite Icon => icon;

        public RarityData RarityData => WeaponsController.GetRarityData(rarity);

        WeaponSave save;
        public WeaponSave Save => save;

        public int CardsAmount => save.CardsAmount;

        public void Initialise()
        {
            save = SaveController.GetSaveObject<WeaponSave>($"Weapon_{type}");
        }
    }
}