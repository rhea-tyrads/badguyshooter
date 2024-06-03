using System;
using System.Collections;
using System.Collections.Generic;
using MobileTools.RateGameUI.Code;
using MobileTools.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDescriptionUI : PopupUI
{
    public TextMeshProUGUI weaponName;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI damage;
    public TextMeshProUGUI radius;
    public TextMeshProUGUI firerate;
    
    public Image weaponImage;
    public Image weaponBackImage;
    public TextMeshProUGUI rarityText;
    public Button selectButton;
    public Button upgradeButton;

    public int Index { get; private set; }

    public event Action OnSelect = delegate { };
    public event Action OnUpgrade = delegate { };

    void Awake()
    {
        selectButton.onClick.AddListener(Click);
        upgradeButton.onClick.AddListener(Upgrade);
    }

    void Click()
    {
        OnSelect();
    }

    void Upgrade()
    {
        OnUpgrade();
    }
    public void SetIndex(int index)
    {
        Index = index;
    }

    public void SetUpgradePossible(bool isPossible)
    {
        if (isPossible)
        {
            upgradeButton.interactable = true;
        }
        else
        {
            upgradeButton.interactable = false;
        }
    }
}