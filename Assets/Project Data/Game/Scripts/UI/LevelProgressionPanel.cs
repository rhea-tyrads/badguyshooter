using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class LevelProgressionPanel : MonoBehaviour
    {
        [SerializeField] Transform levelPreviewContainer;

        [Space]
        [SerializeField] GameObject currentWorldObject;
        [SerializeField] Image currentWorldImage;

        [SerializeField] GameObject nextWorldObject;
        [SerializeField] Image nextWorldImage;

        [Space]
        [SerializeField] RectTransform arrowRectTransform;

        LevelsDatabase levelsDatabase;
        GameSettings levelSettings;

        PreviewCase[] previewCases;

        CanvasGroup canvasGroup;

        TweenCase fadeTweenCase;

        public void Initialise()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            levelSettings = LevelController.LevelSettings;
            levelsDatabase = LevelController.LevelsDatabase;
        }

        public void LoadPanel()
        {
            var currentWorldIndex = ActiveRoom.CurrentWorldIndex;
            var currentLevelIndex = ActiveRoom.CurrentLevelIndex;

            var currentWorld = levelsDatabase.GetWorld(currentWorldIndex);
            var nextWorld = levelsDatabase.GetWorld(currentWorldIndex + 1);

            // Reset pool objects
            if (previewCases != null)
            {
                for (var i = 0; i < previewCases.Length; i++)
                {
                    previewCases[i].Reset();
                }
            }

            // Reset arrow object
            arrowRectTransform.SetParent(transform);

            if (currentWorld != null)
            {
                // Enable panel
                gameObject.SetActive(true);

                // Set current world preview image
                currentWorldImage.sprite = currentWorld.PreviewSprite != null ? currentWorld.PreviewSprite : levelSettings.DefaultWorldSprite;

                // Set next world preview image
                if (nextWorld != null)
                {
                    nextWorldImage.sprite = nextWorld.PreviewSprite != null ? nextWorld.PreviewSprite : levelSettings.DefaultWorldSprite;
                    nextWorldObject.SetActive(true);
                }
                else
                {
                    nextWorldObject.SetActive(false);
                }

                previewCases = new PreviewCase[currentWorld.Levels.Length];
                for (var i = 0; i < previewCases.Length; i++)
                {
                    var levelTypeSettings = levelSettings.GetLevelSettings(currentWorld.Levels[i].Type);

                    var previewObject = levelTypeSettings.PreviewPool.GetPooledObject();
                    previewObject.transform.SetParent(levelPreviewContainer);
                    previewObject.transform.ResetLocal();
                    previewObject.transform.localScale = Vector3.one;
                    previewObject.transform.SetAsLastSibling();

                    previewCases[i] = new PreviewCase(previewObject, levelTypeSettings);

                    if (currentLevelIndex == i)
                    {
                        previewCases[i].PreviewBehaviour.Activate(true);

                        arrowRectTransform.SetParent(previewCases[i].RectTransform);
                        arrowRectTransform.ResetLocal();
                    }
                    else if (currentLevelIndex > i)
                    {
                        previewCases[i].PreviewBehaviour.Complete();
                    }
                    else if (currentLevelIndex < i)
                    {
                        previewCases[i].PreviewBehaviour.Lock();
                    }
                }

                nextWorldObject.transform.SetAsLastSibling();
            }
            else
            {
                // Disable panel
                gameObject.SetActive(false);
            }
        }

        public void Show()
        {
            fadeTweenCase.KillActive();

            fadeTweenCase = canvasGroup.DOFade(1.0f, 0.3f).SetEasing(Ease.Type.CircIn);
        }

        public void Hide()
        {
            fadeTweenCase.KillActive();

            fadeTweenCase = canvasGroup.DOFade(0.0f, 0.3f).SetEasing(Ease.Type.CircIn);
        }

        
    }
}