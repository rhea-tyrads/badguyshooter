using System;
using System.Collections.Generic;
using MobileTools.IAPshop;
using MobileTools.RateGameUI.Code;
using TMPro;
using UnityEngine;


public class SpecialOfferUI : PopupUI
{
    public TextMeshProUGUI timerText;
    public ShopItem shopItem;
    public List<GameObject> models = new();
    void Awake()
    {
        shopItem.OnTryPurchase += Hides;
    }

    protected override void Hiding()
    {
        base.Hiding();
        foreach (var model in models)
        {
            model.SetActive(false);
        }
    }
    
    protected override void Showing()
    {
        base.Hiding();
        foreach (var model in models)
        {
            model.SetActive(true);
        }
    }
    void Hides(ShopItem obj)
    {
        Hide();
    }

    public void RefreshText(TimeSpan remainingTime)
    {
        timerText.text = $"Expired in {remainingTime.Hours:D2}:{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
    }
}