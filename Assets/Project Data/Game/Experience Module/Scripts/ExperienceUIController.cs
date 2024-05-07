using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class ExperienceUIController : MonoBehaviour
    {
        [SerializeField] SlicedFilledImage expProgressFillImage;
        [SerializeField] SlicedFilledImage expProgressBackFillImage;
        [SerializeField] TextMeshProUGUI expLevelText;
        [SerializeField] TextMeshProUGUI expProgressText;
        [SerializeField] RectTransform starsHolder;

        [SerializeField] ExperienceStarsManager starsManager;

        ExperienceController expController;
        int displayedExpPoints;


        public void Init(ExperienceController expController)
        {
            this.expController = expController;

            starsManager.Initialise(this);

            UpdateUI(true);
        }


        #region In Game UI

        int hittedStarsAmount = 0;
        int fixedStarsAmount;
        float currentFillAmount;
        float targetFillAmount;

        TweenCase whiteFillbarCase;

        public void PlayXpGainedAnimation(int starsAmount, Vector3 worldPos, System.Action OnComplete = null)
        {
            hittedStarsAmount = 0;
            fixedStarsAmount = starsAmount;

            var currentLevelExp = expController.CurrentLevelData.ExperienceRequired;
            var requiredExp = expController.NextLevelData.ExperienceRequired;

            targetFillAmount = Mathf.InverseLerp(currentLevelExp, requiredExp, ExperienceController.ExperiencePoints);
            currentFillAmount = expProgressFillImage.fillAmount;

            starsManager.PlayXpGainedAnimation(starsAmount, worldPos, () => UpdateUI(false, OnComplete));
        }

        public void OnStarHitted()
        {
            hittedStarsAmount++;

            if (whiteFillbarCase != null)
                whiteFillbarCase.Kill();

            expProgressBackFillImage.gameObject.SetActive(true);
            whiteFillbarCase = expProgressBackFillImage.DOFillAmount(Mathf.Lerp(currentFillAmount, targetFillAmount, Mathf.InverseLerp(0, fixedStarsAmount, hittedStarsAmount)), 0.1f).SetEasing(Ease.Type.SineIn);
        }

        public void UpdateUI(bool instantly, System.Action OnComplete = null)
        {
            //if (!ExperienceController.IsMax)
            //{
            var currentLevelExp = expController.CurrentLevelData.ExperienceRequired;
            var requiredExp = expController.NextLevelData.ExperienceRequired;

            var firstValue = ExperienceController.ExperiencePoints - currentLevelExp;
            var secondValue = requiredExp - currentLevelExp;

            var fillAmount = Mathf.InverseLerp(currentLevelExp, requiredExp, ExperienceController.ExperiencePoints);
            if (instantly)
            {
                expProgressBackFillImage.fillAmount = fillAmount;
                expProgressFillImage.fillAmount = fillAmount;

                expProgressBackFillImage.gameObject.SetActive(false);

                expLevelText.text = ExperienceController.CurrentLevel.ToString();
                expProgressText.text = firstValue + "/" + secondValue;

                OnComplete?.Invoke();
            }
            else
            {
                RunFillAnimation(fillAmount, secondValue, displayedExpPoints, firstValue, OnComplete);
            }

            displayedExpPoints = firstValue;
            //}
            //else
            //{
            //    expLevelText.text = ExperienceController.CurrentLevel.ToString();

            //    expProgressFillImage.fillAmount = 1.0f;
            //    expProgressText.text = "MAX";

            //    OnComplete?.Invoke();
            //}
        }

        void RunFillAnimation(float newFillAmount, float requiredExp, int displayedExpPoints, int currentExpPoints, System.Action OnComplete = null)
        {
            Tween.DelayedCall(0.5f, () =>
            {
                expProgressFillImage.DOFillAmount(newFillAmount, 0.3f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
                {
                    expLevelText.text = ExperienceController.CurrentLevel.ToString();

                    OnComplete?.Invoke();

                    expProgressBackFillImage.fillAmount = expProgressFillImage.fillAmount;
                    expProgressBackFillImage.gameObject.SetActive(false);
                });

                Tween.DoFloat(displayedExpPoints, currentExpPoints, 0.3f, (value) =>
                {
                    expProgressText.text = (int)value + "/" + requiredExp;
                }).SetEasing(Ease.Type.SineIn);
            });
        }
        #endregion


    }
}