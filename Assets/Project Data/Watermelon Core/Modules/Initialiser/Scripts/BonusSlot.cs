using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusSlot : MonoBehaviour
{
    public TextMeshProUGUI amount;
    int total;
    public void SetAmount(int a)
    {
        total = a;
        amount.text = a.ToString();
    }

    public event Action OnClick = delegate { };
 
    public Button clickButton;

    public Image frame;
    public Image outline;

    public Color framePassiveColor;
    public Color frameActiveColor;

    void Awake()
    {
        clickButton.onClick.AddListener(Click);
    }

    public  void Click()
    {
        OnClick();
    }

    public void Activate()
    {
        amount.text = (total-1).ToString();
        frame.color = frameActiveColor;
        outline.enabled = true;
    }

    public void Disable()
    {
        amount.text = (total).ToString();
        frame.color = framePassiveColor;
        outline.enabled = false;
    }
}