using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Text))]
    public class UILevelNumberText : MonoBehaviour
    {
        const string LEVEL_LABEL = "LEVEL {0}";
        static UILevelNumberText instance;

        [SerializeField] UIScaleAnimation uIScalableObject;

        static UIScaleAnimation UIScalableObject => instance.uIScalableObject;
        static Text levelNumberText;

        static bool IsDisplayed = false;

        void Awake()
        {
            instance = this;
            levelNumberText = GetComponent<Text>();
        }

        void Start()
        {
            UpdateLevelNumber();
        }

        void OnEnable()
        {
        }

        void OnDisable()
        {
        }

        public static void Show(bool immediately = true)
        {
            if (IsDisplayed)
                return;

            IsDisplayed = true;

            levelNumberText.enabled = true;
            UIScalableObject.Show(scaleMultiplier: 1.05f, immediately: immediately);
        }

        public static void Hide(bool immediately = true)
        {
            if (!IsDisplayed)
                return;

            if (immediately)
                IsDisplayed = false;

            UIScalableObject.Hide(scaleMultiplier: 1.05f, immediately: immediately, onCompleted: delegate
           {

               IsDisplayed = false;
               levelNumberText.enabled = false;
           });
        }

        void UpdateLevelNumber()
        {
            levelNumberText.text = string.Format(LEVEL_LABEL, "X");
        }

    }
}
