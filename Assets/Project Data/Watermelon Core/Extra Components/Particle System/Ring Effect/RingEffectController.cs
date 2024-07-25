using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class RingEffectController : MonoBehaviour
    {

        [SerializeField] GameObject ringEffectPrefab;
        [SerializeField] Gradient defaultGradient;
        static RingEffectController _ringEffectController;
        Pool ringEffectPool;

        void Awake()
        {
            _ringEffectController = this;
            ringEffectPool = new Pool(new PoolSettings(ringEffectPrefab.name, ringEffectPrefab, 1, true));
        }

        public static RingEffectCase Spawn(Vector3 position, float targetSize, float time, Ease.Type easing)
            => Spawn(position, _ringEffectController.defaultGradient, targetSize, time, easing);

        public static RingEffectCase Spawn(Vector3 position, Gradient gradient, float targetSize, float time, Ease.Type easing)
        {
            var ringObject = _ringEffectController.ringEffectPool.Get();
            ringObject.transform.position = position;
            ringObject.transform.localScale = Vector3.zero;
            ringObject.SetActive(true);

            var ringEffectCase = new RingEffectCase(ringObject, targetSize, gradient);
            ringEffectCase.SetDuration(time);
            ringEffectCase.SetEasing(easing);
            ringEffectCase.StartTween();

            return ringEffectCase;
        }
    }
}
