using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public static class TutorialHelper
    {
        const string MenuName = "Actions/Skip Tutorial";
        const string SettingName = "IsTutorialSkipped";

        public static bool IsTutorialSkipped()
        {
#if UNITY_EDITOR
            return IsTutorialSkippedPrefs;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        static bool IsTutorialSkippedPrefs
        {
            get { return EditorPrefs.GetBool(SettingName, false); }
            set { EditorPrefs.SetBool(SettingName, value); }
        }

        [MenuItem(MenuName, priority = 200)]
        static void ToggleAction()
        {
            var tutorialState = IsTutorialSkippedPrefs;
            IsTutorialSkippedPrefs = !tutorialState;

            if(Application.isPlaying)
            {
                var type = typeof(TutorialController);

                var field = type.GetField("isTutorialSkipped", BindingFlags.NonPublic | BindingFlags.Static);
                field.SetValue(null, !tutorialState);
            }
        }

        [MenuItem(MenuName, true, priority = 200)]
        static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, IsTutorialSkippedPrefs);

            return true;
        }
#endif
    }
}