using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusPackUI : MonoBehaviour
{
    public ShopBonusItem gold;
    public ShopBonusItem crit;
    public ShopBonusItem hp;
    public ShopBonusItem respawn;

    public void Init(PackData data)
    {
        gold.Set(data.goldIcon, data.goldAmount);
        crit.Set(data.critIcon, data.critAmount);
        hp.Set(data.hpIcon, data.hpAmount);
        respawn.Set(data.respawnIcon, data.respawnAmount);
    }
}