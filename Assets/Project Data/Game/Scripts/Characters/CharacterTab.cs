using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class CharacterTab : MonoBehaviour
    {
        [SerializeField] Image tabImage;
        [SerializeField] Color defaultColor;
        [SerializeField] Color notificationColor;
        [SerializeField] Color disabledColor;
        [SerializeField] GameObject notificationObject;
        UICharactersPanel _characterPanel;
        TweenCase _movementTweenCase;
        Vector2 _defaultAnchoredPosition;
        RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;
        Button _button;
        public Button Button => _button;

        UIGamepadButton _gamepadButton;
        public UIGamepadButton GamepadButton => _gamepadButton;
        CanvasGroup _canvasGroup;
        bool _isActive;

        public void Initialise()
        {
            _button = GetComponent<Button>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _gamepadButton = GetComponent<UIGamepadButton>();

            _rectTransform = (RectTransform)transform;

            _characterPanel = UIController.GetPage<UICharactersPanel>();

            _defaultAnchoredPosition = _rectTransform.anchoredPosition;

            _isActive = true;
        }

        public void OnWindowOpened()
        {
            if (!_isActive)
                return;

            _movementTweenCase.KillActive();

            _rectTransform.anchoredPosition = _defaultAnchoredPosition;
            tabImage.color = defaultColor;

            if (_characterPanel.IsAnyActionAvailable())
            {
                notificationObject.SetActive(true);

                _movementTweenCase = tabImage.DOColor(notificationColor, 0.3f, 0.3f).OnComplete(delegate
                {
                    _movementTweenCase = new TabAnimation(_rectTransform, new Vector2(_defaultAnchoredPosition.x, _defaultAnchoredPosition.y + 30)).SetDuration(1.2f).SetUnscaledMode(false).SetUpdateMethod(UpdateMethod.Update).SetEasing(Ease.Type.QuadOutIn).StartTween();
                });
            }
            else
            {
                notificationObject.SetActive(false);
            }
        }

        public void OnWindowClosed()
        {
            _movementTweenCase.KillActive();

            _rectTransform.anchoredPosition = _defaultAnchoredPosition;
        }

        public void Disable()
        {
            _isActive = false;

            tabImage.color = disabledColor;
            _rectTransform.anchoredPosition = _defaultAnchoredPosition;

            notificationObject.SetActive(false);

            _canvasGroup.alpha = 0.5f;

            _movementTweenCase.KillActive();
        }

        public void Activate()
        {
            _isActive = true;

            _canvasGroup.alpha = 1.0f;

            OnWindowOpened();
        }

        public void OnButtonClicked()
        {
            UIController.HidePage<UIMainMenu>(UIController.ShowPage<UICharactersPanel>);

            AudioController.Play(AudioController.Sounds.buttonSound);
        }
    }
}