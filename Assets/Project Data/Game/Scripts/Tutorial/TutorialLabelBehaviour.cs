using TMPro;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [RequireComponent(typeof(Animation))]
    public class TutorialLabelBehaviour : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI label;

        Animation labelAnimation;

        Transform parentTransform;
        Vector3 offset;

        void Awake()
        {
            labelAnimation = GetComponent<Animation>();
        }

        void Update()
        {
            transform.position = parentTransform.position + offset;
        }

        public void Activate(string text, Transform parentTransform, Vector3 offset)
        {
            this.parentTransform = parentTransform;
            this.offset = offset;

            label.text = text;

            gameObject.SetActive(true);
            labelAnimation.enabled = true;
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            labelAnimation.enabled = false;
        }
    }
}