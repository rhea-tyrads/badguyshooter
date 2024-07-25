using MobileTools.RateGameUI.Code;
using MobileTools.SDK;
using MobileTools.Utilities;
using UnityEngine.UI;


public class GameFullyCompleteUI : PopupUI
{
    public RateGame rateGame;
    public Button reviewButton;

    void Start()
    {
        SDKEvents.Instance.OnShowFullGameCOmplete += Show;
        Hide();
    }

    protected override void Showing()
    {
        if (Keys.IsRated)
        {
            reviewButton.gameObject.SetActive(false);
        }
        else
        {
            reviewButton.gameObject.SetActive(true);
            reviewButton.onClick.AddListener(Review);
        }
    }

    void Review()
    {
        rateGame.Show();
        Hide();
    }
}