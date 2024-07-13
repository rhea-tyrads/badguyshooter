using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.LevelSystem;

public class CheatMoney : MonoBehaviour
{
    public bool use;

    void Update()
    {
        if (!use) return;
        use = false;
        CurrenciesController.Add(CurrencyType.Coins, 55500);
 
    }
}
