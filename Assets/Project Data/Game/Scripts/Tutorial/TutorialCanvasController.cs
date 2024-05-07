using UnityEngine;

namespace Watermelon
{
    public class TutorialCanvasController : MonoBehaviour
    {
        static TutorialCanvasController instance;

        public static readonly int POINTER_DEFAULT = Animator.StringToHash("Default");
        public static readonly int POINTER_TOPDOWN = Animator.StringToHash("Top Down");

        [SerializeField] CanvasGroup fadeCanvasGroup;

        [Space]
        [SerializeField] Animator pointerAnimator;

        static Canvas tutorialCanvas;
        static bool isActive;

        static TransformCase activeTransformCase;

        static TweenCase fadeTweenCase;

        public void Initialise()
        {
            instance = this;

            tutorialCanvas = GetComponent<Canvas>();
            tutorialCanvas.enabled = false;
        }

        public static void ActivatePointer(Vector3 position, int animationHash)
        {
            var pointerTransform = instance.pointerAnimator.transform;
            pointerTransform.gameObject.SetActive(true);
            pointerTransform.position = position;
            pointerTransform.SetAsLastSibling();

            instance.pointerAnimator.Play(animationHash, -1, 0);
        }

        public static void ActivateTutorialCanvas(RectTransform element, bool createDummy, bool fadeImage)
        {
            if (isActive)
                return;

            isActive = true;

            activeTransformCase = new TransformCase(element);
            activeTransformCase.SetNewParent(tutorialCanvas.transform, createDummy);

            tutorialCanvas.enabled = true;

            if (fadeImage)
            {
                instance.fadeCanvasGroup.gameObject.SetActive(true);
                instance.fadeCanvasGroup.alpha = 0;

                fadeTweenCase = instance.fadeCanvasGroup.DOFade(1.0f, 0.3f);
            }
        }

        public static void ResetTutorialCanvas()
        {
            if (!isActive)
                return;

            activeTransformCase.Reset();
            activeTransformCase = null;

            fadeTweenCase.KillActive();

            instance.fadeCanvasGroup.alpha = 0;
            instance.fadeCanvasGroup.gameObject.SetActive(false);

            instance.pointerAnimator.gameObject.SetActive(false);

            tutorialCanvas.enabled = false;

            isActive = false;
        }

        class TransformCase
        {
            RectTransform rectTransform;

            Transform parentTransform;

            Vector2 anchoredPosition;
            Vector2 size;
            Vector3 scale;
            Quaternion rotation;

            int siblingIndex;

            GameObject dummyObject;

            public TransformCase(RectTransform element)
            {
                rectTransform = element;

                siblingIndex = element.GetSiblingIndex();

                parentTransform = element.parent;

                anchoredPosition = element.anchoredPosition;
                size = element.sizeDelta;
                scale = element.localScale;
                rotation = element.localRotation;
            }

            public void SetNewParent(Transform transform, bool createDummy)
            {
                if(createDummy)
                {
                    dummyObject = new GameObject("[TUTORIAL DUMMY]", typeof(RectTransform));
                    dummyObject.transform.SetParent(parentTransform);
                    dummyObject.transform.SetSiblingIndex(siblingIndex);

                    var dummyRectTransform = (RectTransform)dummyObject.transform;
                    dummyRectTransform.anchoredPosition = anchoredPosition;
                    dummyRectTransform.sizeDelta = size;
                    dummyRectTransform.localScale = scale;
                    dummyRectTransform.localRotation = rotation;

                    dummyObject.SetActive(true);
                }

                rectTransform.SetParent(transform, true);
            }

            public void Reset()
            {
                if (dummyObject != null)
                    Destroy(dummyObject);

                rectTransform.SetParent(parentTransform, true);
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.sizeDelta = size;
                rectTransform.localScale = scale;
                rectTransform.localRotation = rotation;

                rectTransform.SetSiblingIndex(siblingIndex);
            }
        }
    }
}
