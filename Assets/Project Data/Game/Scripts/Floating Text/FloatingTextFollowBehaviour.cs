using TMPro;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class FloatingTextFollowBehaviour : FloatingTextBaseBehaviour
    {
        [SerializeField] TextMeshProUGUI floatingText;

        [Space]
        [SerializeField] Vector3 offset;
        [SerializeField] float time;
        [SerializeField] Ease.Type easing;

        [Space]
        [SerializeField] float scaleTime;
        [SerializeField] Ease.Type scaleEasing;

        Vector3 defaultScale;

        void Awake()
        {
            defaultScale = transform.localScale;
        }

        public override void Activate(string text, float scale = 1.0f)
        {
            floatingText.text = text;

            transform.localScale = Vector3.zero;
            transform.DOScale(defaultScale * scale, scaleTime).SetEasing(scaleEasing);
            transform.DOMove(transform.position + offset, time).SetEasing(easing).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        }
    }
}