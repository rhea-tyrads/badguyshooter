using UnityEngine;

namespace Watermelon.Upgrades
{
    public abstract class Upgrade<T> : BaseUpgrade where T : BaseUpgradeStage
    {
        [SerializeField]
        protected T[] upgrades;
        public override BaseUpgradeStage[] Upgrades => upgrades;


        protected T FirstStage => upgrades[1];

        protected T MaxStage => upgrades[^1];

        public T GetCurrentStage()
        {
            if (upgrades.IsInRange(UpgradeLevel))
                return upgrades[UpgradeLevel];
            UpgradeLevel = upgrades.Length - 1;
            Debug.Log("[Perks]: Perk level is out of range!");
            return upgrades[UpgradeLevel];
        }

        public T GetNextStage() 
            => upgrades.IsInRange(UpgradeLevel + 1) ? upgrades[UpgradeLevel + 1] : null;

        [Button("Upgrade")]
        public override void UpgradeStage()
        {
            if (!upgrades.IsInRange(UpgradeLevel + 1)) return;
            UpgradeLevel ++;
            InvokeOnUpgraded();
        }

        public T GetStage(int i) 
            => upgrades.IsInRange(i) ? upgrades[i] : null;

        public void TestUpgrade()
        {
            UpgradeStage();
        }
    }
}