using System.Collections.Generic;
using System.Linq;
using com.adjust.sdk;
using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class WeaponsController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] WeaponDatabase database;

        [Header("Drop")] [SerializeField] GameObject cardPrefab;

        static WeaponsController instance;
        static GlobalWeaponsSave save;
        static List<BaseWeaponUpgradeStage> keyUpgradeStages = new();
        static WeaponData[] weapons;
        static Dictionary<WeaponType, int> weaponsLink;
        UIWeaponPage weaponPageUI;

        public static WeaponDatabase Database => instance.database;
        public static int BasePower { get; private set; }

        public static int SelectedWeaponIndex
        {
            get => save.selectedWeaponIndex;
            private set => save.selectedWeaponIndex = value;
        }

        public static event SimpleCallback OnNewWeaponSelected;
        public static event SimpleCallback OnOpenGunInfo;
        public static event SimpleCallback OnWeaponUpgraded;
        public static event SimpleCallback OnWeaponCardsAmountChanged;
        public static event WeaponDelagate OnWeaponUnlocked;

        #endregion

        public void Initialise()
        {
            instance = this;
            save = SaveController.GetSaveObject<GlobalWeaponsSave>("weapon_save");
            Drop.RegisterDropItem(new CustomDropItem(DropableItemType.WeaponCard, cardPrefab));
            weaponsLink = new Dictionary<WeaponType, int>();
            weapons = database.Weapons;
            for (var i = 0; i < weapons.Length; i++)
            {
                weapons[i].Initialise();
                weaponsLink.Add(weapons[i].Type, i);

                var baseUpgrade = UpgradesController.Get<BaseWeaponUpgrade>(weapons[i].UpgradeType);
                for (var j = 0; j < baseUpgrade.UpgradesCount; j++)
                {
                    var currentStage = baseUpgrade.Upgrades[j] as BaseWeaponUpgradeStage;
                    if (currentStage.KeyUpgradeNumber != -1)
                        keyUpgradeStages.Add(currentStage);
                    if (currentStage.KeyUpgradeNumber == 0)
                        BasePower = currentStage.Power;
                }
            }

            weaponPageUI = UIController.GetPage<UIWeaponPage>();
            weaponPageUI.SetWeaponsController(this);
            keyUpgradeStages.OrderBy(s => s.KeyUpgradeNumber);
            CheckWeaponUpdateState();
            description.OnSelect += SelectWeapon;
            description.OnUpgrade += UpgradeSelected;
            description.Hide();
        }

        public static int GetCeilingKeyPower(int currentKeyUpgrade)
        {
            for (var i = keyUpgradeStages.Count - 1; i >= 0; i--)
            {
                if (keyUpgradeStages[i].KeyUpgradeNumber <= currentKeyUpgrade)
                {
                    return keyUpgradeStages[i].Power;
                }
            }

            return keyUpgradeStages[0].Power;
        }

        public void CheckWeaponUpdateState()
        {
            foreach (var data in weapons)
            {
                var upgrade = UpgradesController.Get<BaseUpgrade>(data.UpgradeType);
                if (upgrade.UpgradeLevel != 0 || data.CardsAmount < upgrade.NextStage.Price) continue;
                upgrade.UpgradeStage();
                OnWeaponUnlocked?.Invoke(data);
            }
        }

        public static void Select(WeaponType weaponType)
        {
            var weaponIndex = 0;
            for (var i = 0; i < instance.database.Weapons.Length; i++)
            {
                if (instance.database.Weapons[i].Type != weaponType) continue;
                weaponIndex = i;
                break;
            }

            instance.OnSelected(weaponIndex);
        }

        public static bool IsTutorialWeaponUpgraded()
        {
            var upg = UpgradesController.Get<BaseUpgrade>(UpgradeType.Minigun);
            return upg.UpgradeLevel >= 2;
        }

        public WeaponDescriptionUI description;

        public void OnSelected(int weaponIndex)
        {
            SelectedWeaponIndex = weaponIndex;

            var weapon = instance.database.GetWeaponByIndex(weaponIndex);
            var page = UIController.GetPage<UIWeaponPage>();
            var panel = page.GetPanel(weapon.Type);
            var data = panel.Data;
            var upgrade = UpgradesController.GetByType(data.UpgradeType);
            //   var stats = upgrade.GetCurrentStage();

            if (upgrade.IsMaxedOut)
            {
                description.NotPossible();
                // description.IsEnoughMoney(false);
            }
            else
            {
                var price = upgrade.NextStage.Price;
                var currencyType = upgrade.NextStage.CurrencyType;
                var canUpgrade = CurrenciesController.Has(currencyType, price);
                description.IsEnoughMoney(canUpgrade);
                description.Possibile(upgrade.NextStage.Price);
            }


            description.weaponName.text = data.Name;
            description.weaponImage.sprite = data.Icon;
            description.weaponBackImage.color = data.RarityData.MainColor;
            description.rarityText.text = data.RarityData.Name;
            description.rarityText.color = data.RarityData.TextColor;
            description.descriptionText.text = data.Description;
            description.levelText.text = "LVL. " + upgrade.UpgradeLevel;
            description.damage.text = "DAMAGE " + Damage(weapon.Type);
            description.firerate.text = "FIRERATE " + FireRate(weapon.Type);
            description.radius.text = "RANGE " + Radius(weapon.Type);
            description.SetIndex(weaponIndex);
            description.Show();

            Debug.Log("WEAPON SELECTED: " + weapon.Type);
            OnOpenGunInfo?.Invoke();
            return;

            SelectedWeaponIndex = weaponIndex;
            CharacterBehaviour.GetBehaviour().SetGun(GetCurrentWeapon(), true);
            CharacterBehaviour.GetBehaviour().Graphics.Grunt();
        }

        void SelectWeapon()
        {
            SelectedWeaponIndex = description.Index;
            CharacterBehaviour.GetBehaviour().SetGun(GetCurrentWeapon(), true);
            CharacterBehaviour.GetBehaviour().Graphics.Grunt();
            OnNewWeaponSelected?.Invoke();
        }

        void UpgradeSelected()
        {
            var weapon = instance.database.GetWeaponByIndex(SelectedWeaponIndex);
//            Debug.LogError(SelectedWeaponIndex);

            var page = UIController.GetPage<UIWeaponPage>();
            var panel = page.GetPanel(weapon.Type);
            panel.UpgradePlease();

            var data = panel.Data;
            var upgrade = UpgradesController.GetByType(data.UpgradeType);

            description.levelText.text = "LVL. " + upgrade.UpgradeLevel;
            description.damage.text = "DAMAGE " + Damage(weapon.Type);
            description.firerate.text = "FIRERATE " + FireRate(weapon.Type);
            description.radius.text = "RANGE " + Radius(weapon.Type);
            Debug.LogWarning("____" + upgrade.UpgradeLevel + " / " + upgrade.Upgrades.Length);
            if (upgrade.NextStage == null)
            {
                description.NotPossible();
            }
            else
            {
                description.upgradePriceTxt.text = upgrade.NextStage.Price.ToString();
            }

            SendAdjustEvent();
        }

        void SendAdjustEvent()
        {
            var weapon = instance.database.GetWeaponByIndex(SelectedWeaponIndex);
            var token = weapon.Type switch
            {
                WeaponType.CrossBow => "cqttkc",
                WeaponType.Flamethrower => "38achz",
                WeaponType.Laser => "sfh9j8",
                WeaponType.LavaLauncher => "8464n9",
                WeaponType.Minigun => "mja0ls",
                WeaponType.PoisonGun => "ht3riw",
                WeaponType.Revolver => "qqksvy",
                WeaponType.Shotgun => "8dozub",
                WeaponType.TeslaGun => "wlg785",
                _ => string.Empty
            };

            if (token.Equals(string.Empty)) return;
            var send = new AdjustEvent(token);
            Adjust.trackEvent(send);
        }

        public static void AddCard(WeaponType weaponType, int amount)
        {
            foreach (var data in weapons)
            {
                if (data.Type != weaponType) continue;
                data.Save.CardsAmount += amount;

                break;
            }

            OnWeaponCardsAmountChanged?.Invoke();
        }

        public static void AddCards(List<WeaponType> cards)
        {
            if (cards.IsNullOrEmpty()) return;
            foreach (var weapon in cards.Select(type => weapons[weaponsLink[type]]))
                weapon.Save.CardsAmount++;
            OnWeaponCardsAmountChanged?.Invoke();
        }

        public static WeaponData GetCurrentWeapon() => instance.database.GetWeaponByIndex(save.selectedWeaponIndex);

        public static WeaponData GetWeaponData(WeaponType weaponType) => instance.database.GetWeapon(weaponType);

        public static RarityData GetRarityData(Rarity rarity) => instance.database.GetRarity(rarity);

        public void WeaponUpgraded(WeaponData weaponData)
        {
            AudioController.Play(AudioController.Sounds.upgrade);
            var characterBehaviour = CharacterBehaviour.GetBehaviour();
            characterBehaviour.SetGun(GetCurrentWeapon(), true, true, true);
            OnWeaponUpgraded?.Invoke();
        }

        public static void UnlockAllWeaponsDev()
        {
            foreach (var data in instance.database.Weapons)
            {
                var upgrade = UpgradesController.Get<BaseUpgrade>(data.UpgradeType);
                if (upgrade.UpgradeLevel == 0)
                    upgrade.UpgradeStage();
            }
        }

        public void Unlock(WeaponType weaponType)
        {
            if (weaponType == WeaponType.Dummy) return;
            if (IsWeaponUnlocked(weaponType)) return;

            var data = instance.database.Weapons.Find(w => w.Type == weaponType);
            var upgrade = UpgradesController.Get<BaseUpgrade>(data.UpgradeType);
            if (upgrade.UpgradeLevel == 0)
                upgrade.UpgradeStage();

            UIController.GetPage<UIWeaponPage>().UpdateUI();
        }

        public static BaseWeaponUpgrade GetCurrentWeaponUpgrade() =>
            UpgradesController.Get<BaseWeaponUpgrade>(GetCurrentWeapon().UpgradeType);

        public static BaseWeaponUpgrade GetWeaponUpgrade(WeaponType type) =>
            UpgradesController.Get<BaseWeaponUpgrade>(GetWeaponData(type).UpgradeType);

        #region Access

        int Damage(WeaponType type) => GetBase(type).Damage.firstValue;
        int FireRate(WeaponType type) => (int)GetBase(type).FireRate;
        int Radius(WeaponType type) => (int)GetBase(type).RangeRadius;

        BaseWeaponUpgradeStage GetUpgrade(UpgradeType type) =>
            type switch
            {
                UpgradeType.Minigun => UpgradesController.Get<MinigunUpgrade>(type).GetCurrentStage(),
                UpgradeType.Shotgun => UpgradesController.Get<ShotgunUpgrade>(type).GetCurrentStage(),
                UpgradeType.Tesla => UpgradesController.Get<TeslaGunUpgrade>(type).GetCurrentStage(),
                UpgradeType.LavaLauncher => UpgradesController.Get<LavaLauncherUpgrade>(type).GetCurrentStage(),
                UpgradeType.Revolver => UpgradesController.Get<RevolverUpgrade>(type).GetCurrentStage(),
                UpgradeType.Laser => UpgradesController.Get<LaserUpgrade>(type).GetCurrentStage(),
                UpgradeType.CrossBow => UpgradesController.Get<CrossbowUpgrade>(type).GetCurrentStage(),
                UpgradeType.PoisonGun => UpgradesController.Get<PoisonGunUpgrade>(type).GetCurrentStage(),
                UpgradeType.Flamethrower => UpgradesController.Get<FlameThrowerUpgrade>(type).GetCurrentStage(),
                _ => UpgradesController.Get<MinigunUpgrade>(type).GetCurrentStage()
            };

        BaseWeaponUpgradeStage GetBase(WeaponType type) =>
            type switch
            {
                WeaponType.Minigun => GetUpgrade(UpgradeType.Minigun),
                WeaponType.LavaLauncher => GetUpgrade(UpgradeType.LavaLauncher),
                WeaponType.TeslaGun => GetUpgrade(UpgradeType.Tesla),
                WeaponType.Shotgun => GetUpgrade(UpgradeType.Shotgun),
                WeaponType.Revolver => GetUpgrade(UpgradeType.Revolver),
                WeaponType.Laser => GetUpgrade(UpgradeType.Laser),
                WeaponType.CrossBow => GetUpgrade(UpgradeType.CrossBow),
                WeaponType.PoisonGun => GetUpgrade(UpgradeType.PoisonGun),
                WeaponType.Flamethrower => GetUpgrade(UpgradeType.Flamethrower),
                _ => GetUpgrade(UpgradeType.Minigun)
            };

        public static bool IsWeaponUnlocked(WeaponType type)
            => (from data in instance.database.Weapons
                where data.Type == type
                select UpgradesController.Get<BaseUpgrade>(data.UpgradeType)
                into upgrade
                select upgrade.UpgradeLevel > 0).FirstOrDefault();

        #endregion


        [System.Serializable]
        public class GlobalWeaponsSave : ISaveObject
        {
            public int selectedWeaponIndex;

            public GlobalWeaponsSave()
            {
                selectedWeaponIndex = 0;
            }

            public void Flush()
            {
            }
        }

        public delegate void WeaponDelagate(WeaponData weapon);
    }
}