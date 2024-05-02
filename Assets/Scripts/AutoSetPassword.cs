#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Android
{
    [ExecuteInEditMode]
    public class AutoSetPassword : MonoBehaviour
    {
        public bool autoSetInEditor;
        public string password;

        void Start()
        {
            if (!autoSetInEditor) return;
            if (password == string.Empty) return;
            PlayerSettings.Android.keystorePass = password;
            PlayerSettings.Android.keyaliasPass = password;
        }
    }
}

#endif