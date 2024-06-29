using System;
using Applovin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusSlot : MonoBehaviour
{
    public TextMeshProUGUI amount;
    int total;

    public event Action<BonusSlot> OnClick = delegate { };
    public Button clickButton;
    public Image frame;
    public Image outline;
    public Color framePassiveColor;
    public Color frameActiveColor;
    public GameObject adsMode;

    void Awake()
    {
        clickButton.onClick.AddListener(Click);
    }

    public void SetAmount(int a)
    {
        total = a;
        amount.text = a.ToString();
        return;
        
        if (a <= 0)
        {
            if (ApplovinController.Instance.IsRewardedLoaded)
            {
                adsMode.SetActive(true);
            }
            else
            {
                amount.text = a.ToString();
                adsMode.SetActive(false);
            }
        }
        else
        {
            amount.text = a.ToString();
            adsMode.SetActive(false);
        }
    }


    public void Click()
    {
        OnClick(this);
    }

    public void Activate()
    {
        amount.text = (total - 1).ToString();
        frame.color = frameActiveColor;
        outline.enabled = true;
    }

    public int incrementedBy;

    public void Increment()
    {
        incrementedBy++;
        total++;
        Activate();
    }

    public void Disable()
    {
        amount.text = (total).ToString();
        frame.color = framePassiveColor;
        outline.enabled = false;
    }
}