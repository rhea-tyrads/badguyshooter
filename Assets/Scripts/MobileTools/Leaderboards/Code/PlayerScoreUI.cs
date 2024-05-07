using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileTools.Leaderboards.Code
{
    public class PlayerScoreUI : MonoBehaviour
    {
        public TextMeshProUGUI playerNameTxt;
        public TextMeshProUGUI scoreTxt;
        public TextMeshProUGUI positionTxt;
        public Image medal;
        public Image frame;
        public Color normalFrame;
        public Color highlightFrame;

        public void Normal()
        {
            frame.color = normalFrame;
            playerNameTxt.color = Color.white;
            scoreTxt.color = Color.white;
            positionTxt.color = Color.white;
        }

        public void Highlight()
        {
            frame.color = highlightFrame;
            //playerNameTxt.color = Color.green;
            //scoreTxt.color = Color.green;
            //positionTxt.color = Color.green;
        }

        public void ShowMedal(Sprite sprite)
        {
            medal.sprite = sprite;
            medal.gameObject.SetActive(true);
        }

        public void HideMedal()
        {
            medal.gameObject.SetActive(false);
        }

        public void SetDetails(string playerName, int position, int score)
        {
            playerNameTxt.text = playerName;
            scoreTxt.text = score.ToString();
            positionTxt.text = position.ToString();
        }


    }
}
