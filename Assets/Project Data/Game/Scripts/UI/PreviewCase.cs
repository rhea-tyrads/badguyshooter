using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class PreviewCase
    {
        GameObject gameObject;
        LevelTypeSettings levelTypeSettings;

        RectTransform rectTransform;
        public RectTransform RectTransform => rectTransform;

        LevelPreviewBaseBehaviour previewBehaviour;
        public LevelPreviewBaseBehaviour PreviewBehaviour => previewBehaviour;

        public PreviewCase(GameObject gameObject, LevelTypeSettings levelTypeSettings)
        {
            this.gameObject = gameObject;
            this.levelTypeSettings = levelTypeSettings;

            rectTransform = (RectTransform)gameObject.transform;

            previewBehaviour = gameObject.GetComponent<LevelPreviewBaseBehaviour>();
            previewBehaviour.Initialise();
        }

        public void Reset()
        {
            gameObject.SetActive(false);
        }
    }
}
