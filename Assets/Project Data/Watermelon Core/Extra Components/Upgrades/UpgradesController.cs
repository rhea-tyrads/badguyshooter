using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    using Upgrades;

    public class UpgradesController : MonoBehaviour
    {
   
        [SerializeField] UpgradesDatabase upgradesDatabase;
        static BaseUpgrade[] _activeUpgrades;
        public static BaseUpgrade[] ActiveUpgrades => _activeUpgrades;
        static Dictionary<UpgradeType, BaseUpgrade> _activeUpgradesLink;
        const string SAVE_IDENTIFIER = "upgrades:{0}";
        
        public void Initialise()
        {
            _activeUpgrades = upgradesDatabase.Upgrades;
            _activeUpgradesLink = new Dictionary<UpgradeType, BaseUpgrade>();
            foreach (var upgrade in _activeUpgrades)
            {
                var hash = string.Format(SAVE_IDENTIFIER, upgrade.UpgradeType.ToString()).GetHashCode();
                var save = SaveController.GetSaveObject<UpgradeSavableObject>(hash); ;
                upgrade.SetSave(save);
                if (_activeUpgradesLink.ContainsKey(upgrade.UpgradeType)) continue;
                upgrade.Initialise();
                _activeUpgradesLink.Add(upgrade.UpgradeType, upgrade);
            }
        }

        [System.Obsolete]
        public static BaseUpgrade GetByType(UpgradeType perkType)
        {
            if (_activeUpgradesLink.ContainsKey(perkType))
                return _activeUpgradesLink[perkType];
            Debug.LogError($"[Perks]: Upgrade with type {perkType} isn't registered!");
            return null;
        }

        public static T Get<T>(UpgradeType type) where T : BaseUpgrade
        {
            if (_activeUpgradesLink.ContainsKey(type))
                return _activeUpgradesLink[type] as T;
            Debug.LogError($"[Perks]: Upgrade with type {type} isn't registered!");
            return null;
        }
    }
}