using System.Linq;
using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class UIWeaponPage : UIUpgradesAbstractPage<WeaponPanelUI, WeaponType>
    {
        WeaponsController weaponController;

        protected override int SelectedIndex
            => Mathf.Clamp(WeaponsController.SelectedWeaponIndex, 0, int.MaxValue);

        public void SetWeaponsController(WeaponsController weaponController) 
            => this.weaponController = weaponController;

        public void UpdateUI()
            => itemPanels.ForEach(panel => panel.UpdateUI());

        public override WeaponPanelUI GetPanel(WeaponType weaponType) 
            => itemPanels.FirstOrDefault(panel => panel.Data.Type == weaponType);

        public bool IsAnyActionAvailable() 
            => itemPanels.Any(panel => panel.IsNextUpgradeCanBePurchased());

        #region UI Page

        public override void Initialise()
        {
            base.Initialise();

            for (var i = 0; i < WeaponsController.Database.Weapons.Length; i++)
            {
                var weapon = WeaponsController.Database.Weapons[i];
                var upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(weapon.UpgradeType);
                var newPanel = AddNewPanel();
                newPanel.Init(weaponController, upgrade as BaseWeaponUpgrade, weapon, i, weapon.availableOnlyInShop);
            }

            WeaponsController.OnWeaponUnlocked += (_) => UpdateUI();
            WeaponsController.OnWeaponUpgraded += UpdateUI;
        }

        public override void PlayShowAnimation()
        {
            base.PlayShowAnimation();

            UpdateUI();
            OverlayUI.ShowOverlay();
        }

        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation();
            UIController.OnPageClosed(this);
        }

        protected override void HidePage(SimpleCallback onFinish) 
            => UIController.HidePage<UIWeaponPage>(onFinish);

        #endregion
        
    }
}