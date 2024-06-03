using System;
using MobileTools.MonoCache.System;

namespace MobileTools.SDK
{
    public class SDKEvents : Singleton<SDKEvents>
    {
    
        public event Action<int,int> OnLevelComplete = delegate { };
        public void LevelComplete(int world, int room) => OnLevelComplete(world,room);
        public event Action<string> OnProductPurchase = delegate { };
        public void ProductPurchase(string id) => OnProductPurchase(id);
    }
}