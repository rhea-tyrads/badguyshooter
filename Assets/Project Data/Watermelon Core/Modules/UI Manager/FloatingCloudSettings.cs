using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class FloatingCloudSettings
    {
        public const float DEFAULT_RADIUS = 200;

        string name;
        public string Name => name;

        float cloudRadius;
        public float CloudRadius => cloudRadius;

        GameObject prefab;
        public GameObject Prefab => prefab;

        Sprite sprite;
        public Sprite Sprite => sprite;

        AudioClip appearAudioClip;
        public AudioClip AppearAudioClip => appearAudioClip;

        AudioClip collectAudioClip;
        public AudioClip CollectAudioClip => collectAudioClip;

        public FloatingCloudSettings(string name, GameObject prefab)
        {
            this.name = name;
            this.prefab = prefab;
            this.cloudRadius = DEFAULT_RADIUS;
        }

        public FloatingCloudSettings(string name, Sprite sprite, Vector2 size)
        {
            this.name = name;
            this.cloudRadius = DEFAULT_RADIUS;

            var tempPrefab = new GameObject(name);
            tempPrefab.hideFlags = HideFlags.HideInHierarchy;

            var image = tempPrefab.AddComponent<Image>();
            image.sprite = sprite;

            var rectTransform = (RectTransform)tempPrefab.transform;
            rectTransform.sizeDelta = size;

            this.prefab = tempPrefab;
        }

        public FloatingCloudSettings SetAudio(AudioClip appearAudioClip, AudioClip collectAudioClip)
        {
            this.appearAudioClip = appearAudioClip;
            this.collectAudioClip = collectAudioClip;

            return this;
        }

        public FloatingCloudSettings SetRadius(float cloudRadius)
        {
            this.cloudRadius = cloudRadius;

            return this;
        }
    }
}
