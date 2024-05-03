using UnityEngine;

namespace FortuneWheels.Scripts
{
    [System.Serializable]
    public class DailyReward
    {
        public DailyRewardType type;
        public int amount;
        [HideInInspector] public Sprite icon;
    }
}