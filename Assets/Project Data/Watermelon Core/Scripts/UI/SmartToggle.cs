#pragma warning disable 0649 

//#define USING_UNITY_EVENT // allowes to subscribe method using inspector, but currently invokes event with wrong parameter | fix needed

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace Watermelon
{
    [RequireComponent(typeof(Image))]
    public class SmartToggle : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] bool defaultState;

        [SerializeField] RectTransform movableElement;
        Graphic movableElementGraphic;

        [Space]
        [SerializeField]
        Text enableText;
        [SerializeField] Text disableText;

        [Space]
        [SerializeField]
        Color activeOnTextColor;
        [SerializeField] Color activeOffTextColor;

        [SerializeField] Color disabledTextColor;

        [Space]
        [SerializeField]
        Graphic icon;

#if USING_UNITY_EVENT
    [SerializeField]
    private StateEvent onStateChanged;
#else
        public delegate void ToggleEvents(bool state);
        public event ToggleEvents OnStateChanged;
#endif

        bool state;
        public bool State
        {
            get { return state; }
        }

        RectTransform rectTransform;
        float offsetValue = 0;

        bool isBusy = false;

        TweenCase animationTweenCase;

        bool isInited = false;

        void Awake()
        {
            if (!isInited)
                Init(defaultState);
        }

        public void Init(bool state)
        {
            movableElementGraphic = movableElement.GetComponent<Graphic>();

            rectTransform = (RectTransform)transform;
            offsetValue = (rectTransform.sizeDelta.x - movableElement.sizeDelta.x) / 2;

            this.state = state;

            movableElement.anchoredPosition = new Vector2(state ? offsetValue : -offsetValue, 0);

            if (state)
            {
                enableText.color = activeOnTextColor;
                disableText.color = disabledTextColor;

                if (icon != null)
                    icon.color = activeOnTextColor;

                movableElementGraphic.color = activeOnTextColor;
            }
            else
            {
                enableText.color = disabledTextColor;
                disableText.color = activeOffTextColor;

                if (icon != null)
                    icon.color = activeOffTextColor;

                movableElementGraphic.color = activeOffTextColor;
            }

            isInited = true;
        }

        public void SetStateImmediately(bool state)
        {
            if (this.state != state)
            {
                if (animationTweenCase != null && !animationTweenCase.IsCompleted)
                    animationTweenCase.Kill();

                this.state = state;

#if USING_UNITY_EVENT
            if (onStateChanged != null)
                onStateChanged.Invoke(state);
#else
                if (OnStateChanged != null)
                    OnStateChanged.Invoke(state);
#endif

                movableElement.anchoredPosition = new Vector2(state ? offsetValue : -offsetValue, 0);

                if (state)
                {
                    enableText.color = activeOnTextColor;
                    disableText.color = disabledTextColor;

                    if (icon != null)
                        icon.color = activeOnTextColor;

                    movableElementGraphic.color = activeOnTextColor;
                }
                else
                {
                    enableText.color = disabledTextColor;
                    disableText.color = activeOffTextColor;

                    if (icon != null)
                        icon.color = activeOffTextColor;

                    movableElementGraphic.color = activeOffTextColor;
                }
            }
        }

        public void SetState(bool state)
        {

            Debug.Log("Toggle. Current state: " + this.state + "  new state: " + state);

            if (isBusy && this.state == state)
                return;

            isBusy = true;

            this.state = state;

#if USING_UNITY_EVENT
        if (onStateChanged != null)
            onStateChanged.Invoke(state);
#else
            if (OnStateChanged != null)
                OnStateChanged.Invoke(state);
#endif

            animationTweenCase = movableElement.DOAnchoredPosition(new Vector3(state ? offsetValue : -offsetValue, 0), 0.1f, 0, true).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
            {
                if (state)
                {
                    enableText.color = activeOnTextColor;
                    disableText.color = disabledTextColor;

                    if (icon != null)
                        icon.color = activeOnTextColor;

                    movableElementGraphic.color = activeOnTextColor;
                }
                else
                {
                    enableText.color = disabledTextColor;
                    disableText.color = activeOffTextColor;

                    if (icon != null)
                        icon.color = activeOffTextColor;

                    movableElementGraphic.color = activeOffTextColor;
                }

                isBusy = false;
            });
        }

        public void Toggle()
        {
            SetState(!state);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetState(!state);

#if MODULE_AUDIO_NATIVE
        NativeAudioHandler.PlayButtonPressSound();
#endif
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //Required for OnPointerUp function
        }


#if USING_UNITY_EVENT
    [System.Serializable]
    public class StateEvent : UnityEvent<bool> { }
#endif

    }
}