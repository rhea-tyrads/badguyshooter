using System;
using MobileTools.RateGameUI.Code;
using MobileTools.SDK;
using TMPro;
using UnityEngine.UI;

public class WeaponDescriptionUI : PopupUI
{
    public TextMeshProUGUI weaponName;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI damage;
    public TextMeshProUGUI radius;
    public TextMeshProUGUI firerate;
    
    public Image weaponImage;
    public Image weaponBackImage;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI upgradePriceTxt;
    public Button selectButton;
    public Button upgradeButton;

    public int Index { get; private set; }

    public event Action OnSelect = delegate { };
    public event Action OnUpgrade = delegate { };

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
        upgradeButton.onClick.AddListener(Upgrade);
    }

    void Start()
    {
        SDKEvents.Instance.OnWeaponUpgradeNotPossibile += NotPossible;
        SDKEvents.Instance.OnWeaponUpgradeMaxed += Maxed;
        SDKEvents.Instance.OnWeaponUpgradePossibile += Possibile;
    }

 public   void Possibile(int cost)
    {
        upgradeButton.gameObject.SetActive(true);
        upgradePriceTxt.text = cost.ToString();
    }

    void Maxed()
    {
        upgradeButton.gameObject.SetActive(false);
    }

  public  void NotPossible()
    {
        upgradeButton.gameObject.SetActive(false);  
    }

    void Select()
    {
        Hide();
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

    public void IsEnoughMoney(bool isPossible)
    {
        upgradeButton.interactable = isPossible;
    }
}