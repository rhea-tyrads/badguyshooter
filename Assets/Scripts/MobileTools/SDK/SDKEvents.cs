using System;
using MobileTools.MonoCache.System;

namespace MobileTools.SDK
{
    public class SDKEvents : Singleton<SDKEvents>
    {
        public event Action OnTryPurchaseNoAds = delegate { };
        public void TryPurchaseNoAds(   ) => OnTryPurchaseNoAds( );
        public event Action<int,int> OnLevelComplete = delegate { };
        public void LevelComplete(int world, int room) => OnLevelComplete(world,room);
        
        
        public event Action  OnShowFullGameCOmplete = delegate { };
        public void ShowFullGameCOmplete( ) => OnShowFullGameCOmplete();
        public event Action<string> OnProductPurchase = delegate { };
        public void ProductPurchase(string id) => OnProductPurchase(id);
        
        public event Action<int> OnWeaponUpgradePossibile = delegate { };
        public void WeaponUpgradePossibile(int price) => OnWeaponUpgradePossibile(price);
        
        public event Action  OnWeaponUpgradeNotPossibile = delegate { };
        public void WeaponUpgradeNotPossibile(   ) => OnWeaponUpgradeNotPossibile( );
        
        public event Action  OnWeaponUpgradeMaxed = delegate { };
        public void WeaponUpgradeMaxed(   ) => OnWeaponUpgradeMaxed( );
    }
}