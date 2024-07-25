using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class DroppedCardPanel : MonoBehaviour
    {
        const string CARDS_TEXT = "{0}/{1}";

        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] Image weaponPreviewImage;
        [SerializeField] Image weaponBackgroundImage;
        [SerializeField] TextMeshProUGUI rarityText;
        [SerializeField] GameObject newRibbonObject;

        [Space]
        [SerializeField] GameObject progressPanelObject;
        [SerializeField] GameObject progressFillbarObject;
        [SerializeField] SlicedFilledImage progressFillbarImage;
        [SerializeField] TextMeshProUGUI progressFillbarText;
        [SerializeField] GameObject progressEquipButtonObject;
        [SerializeField] GameObject progressEquipedObject;

        CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup => _canvasGroup;
        WeaponData _weaponData;
        RarityData _rarityData;
        BaseWeaponUpgrade _weaponUpgrade;

        int _currentCardsAmount;

        public void Initialise(WeaponType weaponType)
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            _weaponData = WeaponsController.GetWeaponData(weaponType);
            _rarityData = WeaponsController.GetRarityData(_weaponData.Rarity);
            _weaponUpgrade = UpgradesController.Get<BaseWeaponUpgrade>(_weaponData.UpgradeType);
            _currentCardsAmount = _weaponData.CardsAmount;
            titleText.text = _weaponData.Name;
            weaponPreviewImage.sprite = _weaponData.Icon;
            rarityText.text = _rarityData.Name;
            rarityText.color = _rarityData.TextColor;
            weaponBackgroundImage.color = _rarityData.MainColor;
            progressPanelObject.SetActive(false);
        }

        public void OnDisplayed()
        {
            var target = _weaponUpgrade.Upgrades[1].Price;

            progressPanelObject.SetActive(true);
            progressFillbarObject.SetActive(true);

            progressEquipButtonObject.SetActive(false);
            progressEquipedObject.SetActive(false);

            progressFillbarText.text = string.Format(CARDS_TEXT, _currentCardsAmount, target);

            progressPanelObject.transform.localScale = Vector3.one * 0.8f;
            progressPanelObject.transform.DOScale(Vector3.one, 0.15f).SetEasing(Ease.Type.BackOut);

            progressFillbarImage.fillAmount = 0.0f;
            progressFillbarImage.DOFillAmount((float)_currentCardsAmount / target, 0.4f, 0.1f).OnComplete(delegate
            {
                if (_currentCardsAmount >= target)
                {
                    Tween.DelayedCall(0.5f, delegate
                    {
                        progressFillbarObject.SetActive(false);

                        progressEquipButtonObject.SetActive(true);
                        progressEquipButtonObject.transform.localScale = Vector3.one * 0.7f;
                        progressEquipButtonObject.transform.DOScale(Vector3.one, 0.25f).SetEasing(Ease.Type.BackOut);
                    });
                }
            });
        }

        public void OnEquipButtonClicked()
        {
            WeaponsController.Select(_weaponData.Type);
            progressEquipButtonObject.SetActive(false);
            progressEquipedObject.SetActive(true);
            AudioController.Play(AudioController.Sounds.buttonSound);
        }
    }
}