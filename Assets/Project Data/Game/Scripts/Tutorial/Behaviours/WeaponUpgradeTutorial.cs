using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class WeaponUpgradeTutorial : ITutorial
    {
        const WeaponType FIRST_WEAPON_TYPE = WeaponType.Revolver;

        public TutorialID TutorialID => TutorialID.WeaponUpgrade;

        const int STEP_TUTORIAL_ACTIVATED = 1;
        const int STEP_PAGE_OPENED = 2;

        public bool IsActive => saveData.isActive;
        public bool IsFinished => saveData.isFinished;
        public int Progress => saveData.progress;

        TutorialBaseSave saveData;
        WeaponData weaponData;
        BaseWeaponUpgrade weaponUpgrade;
        UIMainMenu mainMenuUI;
        UIWeaponPage weaponPageUI;
        WeaponTab weaponTab;
        CharacterTab characterTab;
        bool isActive;
        int stepNumber;
        UIGamepadButton activatedGamepadButton;
        UIGamepadButton noAdsGamepadButton;
        UIGamepadButton settingsGamepadButton;
        UIGamepadButton playGamepadButton;

        bool isInitialised;
        public bool IsInitialised => isInitialised;

        public WeaponUpgradeTutorial() => TutorialController.RegisterTutorial(this);

        public void Initialise()
        {
            if (isInitialised) return;

            isInitialised = true;
            saveData = SaveController.GetSaveObject<TutorialBaseSave>(string.Format(ITutorial.SAVE_IDENTIFIER,
                TutorialID.ToString()));
            weaponData = WeaponsController.GetWeaponData(FIRST_WEAPON_TYPE);
            weaponUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);
            mainMenuUI = UIController.GetPage<UIMainMenu>();
            weaponPageUI = UIController.GetPage<UIWeaponPage>();
            weaponTab = mainMenuUI.WeaponTab;
            characterTab = mainMenuUI.CharacterTab;
            noAdsGamepadButton = mainMenuUI.NoAdsGamepadButton;
            settingsGamepadButton = mainMenuUI.SettingsGamepadButton;
            playGamepadButton = mainMenuUI.PlayGamepadButton;
        }

        public void StartTutorial()
        {
            if (isActive) return;
            isActive = true;
            weaponTab.Disable();
            characterTab.Disable();
            UIController.OnPageOpenedEvent += OnMainMenuPageOpened;
            Control.OnInputChanged += OnInputTypeChanged;
        }

        void OnInputTypeChanged(InputType input)
        {
            if (activatedGamepadButton) activatedGamepadButton.StopHighLight();
            TutorialCanvasController.ResetTutorialCanvas();

            switch (stepNumber)
            {
                case STEP_TUTORIAL_ACTIVATED:
                {
                    TutorialCanvasController.ActivateTutorialCanvas(weaponTab.RectTransform, false, true);

                    if (input == InputType.Gamepad)
                    {
                        activatedGamepadButton = weaponTab.GamepadButton;
                        if (activatedGamepadButton) activatedGamepadButton.StartHighlight();
                        if (characterTab.GamepadButton) characterTab.GamepadButton.SetFocus(false);
                        if (noAdsGamepadButton) noAdsGamepadButton.SetFocus(false);
                        if (settingsGamepadButton) settingsGamepadButton.SetFocus(false);
                        if (playGamepadButton) playGamepadButton.SetFocus(false);
                    }
                    else
                    {
                        TutorialCanvasController.ActivatePointer(
                            weaponTab.RectTransform.position + new Vector3(0, 0.1f, 0),
                            TutorialCanvasController.POINTER_TOPDOWN);
                    }

                    break;
                }
                case STEP_PAGE_OPENED:
                {
                    var weaponPanel = weaponPageUI.GetPanel(FIRST_WEAPON_TYPE);
                    if (weaponPanel)
                    {
                        TutorialCanvasController.ActivateTutorialCanvas(weaponPanel.RectTransform, true, true);

                        if (Control.InputType == InputType.Gamepad)
                        {
                            if (weaponPageUI.GamepadCloseButton) weaponPageUI.GamepadCloseButton.SetFocus(false);
                            if (characterTab.GamepadButton) characterTab.GamepadButton.SetFocus(false);
                            if (noAdsGamepadButton) noAdsGamepadButton.SetFocus(false);
                            if (settingsGamepadButton) settingsGamepadButton.SetFocus(false);
                            if (playGamepadButton) playGamepadButton.SetFocus(false);
                            activatedGamepadButton = weaponTab.GamepadButton;
                            if (activatedGamepadButton) activatedGamepadButton.StartHighlight();
                        }
                        else
                        {
                            TutorialCanvasController.ActivatePointer(weaponPanel.UpgradeButtonTransform.position,
                                TutorialCanvasController.POINTER_TOPDOWN);
                        }
                    }

                    break;
                }
            }
        }

        void OnMainMenuPageOpened(UIPage page, System.Type pageType)
        {
            if (pageType != typeof(UIMainMenu)) return;
            if (ActiveRoom.CurrentLevelIndex < 2) return;
            var stage = weaponUpgrade.NextStage;
            if (stage == null) return;

            // Player has enough money to upgrade first weapon
            if (CurrenciesController.HasAmount(stage.CurrencyType, stage.Price))
            {
                UIController.OnPageOpenedEvent -= OnMainMenuPageOpened;
                stepNumber = STEP_TUTORIAL_ACTIVATED;
                weaponTab.Activate();
                weaponTab.Button.onClick.AddListener(OnWeaponTabOpened);
                TutorialCanvasController.ActivateTutorialCanvas(weaponTab.RectTransform, false, true);

                if (Control.InputType == InputType.Gamepad)
                {
                    activatedGamepadButton = weaponTab.GamepadButton;
                    if (activatedGamepadButton) activatedGamepadButton.StartHighlight();
                    if (characterTab.GamepadButton) characterTab.GamepadButton.SetFocus(false);
                    if (noAdsGamepadButton) noAdsGamepadButton.SetFocus(false);
                    if (settingsGamepadButton) settingsGamepadButton.SetFocus(false);
                    if (playGamepadButton) playGamepadButton.SetFocus(false);
                }
                else
                {
                    TutorialCanvasController.ActivatePointer(weaponTab.RectTransform.position + new Vector3(0, 0.1f, 0),
                        TutorialCanvasController.POINTER_TOPDOWN);
                }
            }
            else
            {
                weaponTab.Disable();
            }
        }

        void OnWeaponTabOpened()
        {
            TutorialCanvasController.ResetTutorialCanvas();
            weaponTab.Button.onClick.RemoveListener(OnWeaponTabOpened);
            UIController.OnPageOpenedEvent += OnWeaponPageOpened;
            weaponPageUI.GraphicRaycaster.enabled = false;
        }

        void OnWeaponPageOpened(UIPage page, System.Type pageType)
        {
            UIController.OnPageOpenedEvent -= OnWeaponPageOpened;

            var weaponPanel = weaponPageUI.GetPanel(FIRST_WEAPON_TYPE);
            if (weaponPanel)
            {
                stepNumber = STEP_PAGE_OPENED;

                TutorialCanvasController.ActivateTutorialCanvas(weaponPanel.RectTransform, true, true);

                if (Control.InputType == InputType.Gamepad)
                {
                    if (weaponPageUI.GamepadCloseButton) weaponPageUI.GamepadCloseButton.SetFocus(false);
                    if (characterTab.GamepadButton) characterTab.GamepadButton.SetFocus(false);
                    if (noAdsGamepadButton) noAdsGamepadButton.SetFocus(false);
                    if (settingsGamepadButton) settingsGamepadButton.SetFocus(false);
                    if (playGamepadButton) playGamepadButton.SetFocus(false);
                    activatedGamepadButton = weaponTab.GamepadButton;
                    if (activatedGamepadButton) activatedGamepadButton.StartHighlight();
                }
                else
                {
                    TutorialCanvasController.ActivatePointer(weaponPanel.UpgradeButtonTransform.position,
                        TutorialCanvasController.POINTER_TOPDOWN);
                }

                Debug.LogError("ZZZZZZZZZZZZZZZ");
                WeaponsController.OnOpenGunInfo += OnWeaponUpgraded;
              //  WeaponsController.OnWeaponUpgraded += OnWeaponUpgraded;

                if (WeaponsController.IsTutorialWeaponUpgraded())
                    OnWeaponUpgraded();
            }

            weaponPageUI.GraphicRaycaster.enabled = true;
        }

        void OnWeaponUpgraded()
        {
            Debug.LogError("AAAAAAAAa");
            WeaponsController.OnOpenGunInfo -= OnWeaponUpgraded;
          //  WeaponsController.OnWeaponUpgraded -= OnWeaponUpgraded;
            TutorialCanvasController.ResetTutorialCanvas();
            if (Control.InputType == InputType.Gamepad)
            {
                if (weaponPageUI.GamepadCloseButton) weaponPageUI.GamepadCloseButton.SetFocus(true);
                if (characterTab.GamepadButton) characterTab.GamepadButton.SetFocus(true);
                if (weaponTab.GamepadButton) weaponTab.GamepadButton.SetFocus(true);
                if (noAdsGamepadButton) noAdsGamepadButton.SetFocus(true);
                if (settingsGamepadButton) settingsGamepadButton.SetFocus(true);
                if (playGamepadButton) playGamepadButton.SetFocus(true);
                if (activatedGamepadButton) activatedGamepadButton.StopHighLight();
            }
            weaponTab.Activate();
            characterTab.Activate();
            FinishTutorial();
        }

        public void FinishTutorial() => saveData.isFinished = true;

        public void Unload()
        {
        }
    }
}