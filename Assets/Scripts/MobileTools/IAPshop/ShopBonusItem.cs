using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopBonusItem : MonoBehaviour
{
    public TextMeshProUGUI amountTxt;
    public Image iconImage;

    public void Set(Sprite icon, int amount)
    {
        iconImage.sprite = icon;
        amountTxt.text = amount.ToString();
    }
}
