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

        ExperienceController _expController;
        int _displayedExpPoints;


        public void Init(ExperienceController expController)
        {
            this._expController = expController;

            starsManager.Initialise(this);

            UpdateUI(true);
        }


        #region In Game UI

        int _hittedStarsAmount = 0;
        int _fixedStarsAmount;
        float _currentFillAmount;
        float _targetFillAmount;

        TweenCase _whiteFillbarCase;

        public void PlayXpGainedAnimation(int starsAmount, Vector3 worldPos, System.Action complete = null)
        {
            _hittedStarsAmount = 0;
            _fixedStarsAmount = starsAmount;

            int currentLevelExp = _expController.CurrentLevelData.ExperienceRequired;
            int requiredExp = _expController.NextLevelData.ExperienceRequired;

            _targetFillAmount = Mathf.InverseLerp(currentLevelExp, requiredExp, ExperienceController.ExperiencePoints);
            _currentFillAmount = expProgressFillImage.fillAmount;

            starsManager.PlayXpGainedAnimation(starsAmount, worldPos, () => UpdateUI(false, complete));
        }

        public void OnStarHitted()
        {
            _hittedStarsAmount++;

            if (_whiteFillbarCase != null)
                _whiteFillbarCase.Kill();

            expProgressBackFillImage.gameObject.SetActive(true);
            _whiteFillbarCase = expProgressBackFillImage.DOFillAmount(Mathf.Lerp(_currentFillAmount, _targetFillAmount, Mathf.InverseLerp(0, _fixedStarsAmount, _hittedStarsAmount)), 0.1f).SetEasing(Ease.Type.SineIn);
        }

        public void UpdateUI(bool instantly, System.Action complete = null)
        {
            //if (!ExperienceController.IsMax)
            //{
            int currentLevelExp = _expController.CurrentLevelData.ExperienceRequired;
            int requiredExp = _expController.NextLevelData.ExperienceRequired;

            int firstValue = ExperienceController.ExperiencePoints - currentLevelExp;
            int secondValue = requiredExp - currentLevelExp;

            float fillAmount = Mathf.InverseLerp(currentLevelExp, requiredExp, ExperienceController.ExperiencePoints);
            if (instantly)
            {
                expProgressBackFillImage.fillAmount = fillAmount;
                expProgressFillImage.fillAmount = fillAmount;

                expProgressBackFillImage.gameObject.SetActive(false);

                expLevelText.text = ExperienceController.CurrentLevel.ToString();
                expProgressText.text = firstValue + "/" + secondValue;

                complete?.Invoke();
            }
            else
            {
                RunFillAnimation(fillAmount, secondValue, _displayedExpPoints, firstValue, complete);
            }

            _displayedExpPoints = firstValue;
            //}
            //else
            //{
            //    expLevelText.text = ExperienceController.CurrentLevel.ToString();

            //    expProgressFillImage.fillAmount = 1.0f;
            //    expProgressText.text = "MAX";

            //    OnComplete?.Invoke();
            //}
        }

        void RunFillAnimation(float newFillAmount, float requiredExp, int displayedExpPoints, int currentExpPoints, System.Action complete = null)
        {
            Tween.DelayedCall(0.5f, () =>
            {
                expProgressFillImage.DOFillAmount(newFillAmount, 0.3f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
                {
                    expLevelText.text = ExperienceController.CurrentLevel.ToString();

                    complete?.Invoke();

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