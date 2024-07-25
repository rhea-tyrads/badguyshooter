using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class UICharactersPanel : UIUpgradesAbstractPage<CharacterPanelUI, CharacterType>
    {
        [Space]
        [SerializeField] GameObject stageStarPrefab;

        CharactersDatabase charactersDatabase;

        Pool stageStarPool;

        protected override int SelectedIndex
            => Mathf.Clamp(CharactersController.GetIndex(CharactersController.SelectedCharacter.Type), 0, int.MaxValue);

        public GameObject GetStageStarObject() => stageStarPool.Get();

        public bool IsAnyActionAvailable()
        {
            foreach (var panel in itemPanels)
            {
                if (panel.IsNewCharacterOpened())
                    return true;

                if (panel.IsNextUpgradeCanBePurchased())
                    return true;
            }

            return false;
        }

        #region Animation

        bool isAnimationPlaying;
        Coroutine animationCoroutine;

        static bool isControlBlocked = false;
        public static bool IsControlBlocked => isControlBlocked;

        static List<CharacterDynamicAnimation> characterDynamicAnimations = new();

        void ResetAnimations()
        {
            if (isAnimationPlaying)
            {
                StopCoroutine(animationCoroutine);

                isAnimationPlaying = false;
                animationCoroutine = null;
            }

            characterDynamicAnimations = new List<CharacterDynamicAnimation>();
        }

        void StartAnimations()
        {
            if (isAnimationPlaying)
                return;

            if (!characterDynamicAnimations.IsNullOrEmpty())
            {
                isControlBlocked = true;
                scrollView.enabled = false;

                isAnimationPlaying = true;

                animationCoroutine = StartCoroutine(DynamicAnimationCoroutine());
            }
        }

        IEnumerator ScrollCoroutine(CharacterPanelUI characterPanelUI)
        {
            var scrollOffsetX = -(characterPanelUI.RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);

            var positionDiff = Mathf.Abs(scrollView.content.anchoredPosition.x - scrollOffsetX);

            if (positionDiff > 80)
            {
                var easeFunctionCubicIn = Ease.GetFunction(Ease.Type.CubicOut);

                var currentPosition = scrollView.content.anchoredPosition;
                var targetPosition = new Vector2(scrollOffsetX, 0);

                var speed = positionDiff / 2500;

                for (float s = 0; s < 1.0f; s += Time.deltaTime / speed)
                {
                    scrollView.content.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, easeFunctionCubicIn.Interpolate(s));

                    yield return null;
                }
            }
        }

        IEnumerator DynamicAnimationCoroutine()
        {
            var currentAnimationIndex = 0;
            CharacterDynamicAnimation tempAnimation;
            var delayWait = new WaitForSeconds(0.4f);

            yield return delayWait;

            while (currentAnimationIndex < characterDynamicAnimations.Count)
            {
                tempAnimation = characterDynamicAnimations[currentAnimationIndex];

                delayWait = new WaitForSeconds(tempAnimation.delay);

                yield return StartCoroutine(ScrollCoroutine(tempAnimation.characterPanel));

                tempAnimation.AnimationStarted?.Invoke();

                yield return delayWait;

                currentAnimationIndex++;
            }

            yield return null;

            isAnimationPlaying = false;
            isControlBlocked = false;
            scrollView.enabled = true;
        }

        public void AddAnimations(List<CharacterDynamicAnimation> characterDynamicAnimation, bool isPrioritize = false)
        {
            if (!isPrioritize)
            {
                characterDynamicAnimations.AddRange(characterDynamicAnimation);
            }
            else
            {
                characterDynamicAnimations.InsertRange(0, characterDynamicAnimation);
            }
        }

        #endregion

        #region UI Page

        public override void Initialise()
        {
            base.Initialise();

            charactersDatabase = CharactersController.GetDatabase();

            stageStarPool = new Pool(new PoolSettings(stageStarPrefab.name, stageStarPrefab, 1, true));

            foreach (var c in charactersDatabase.Characters)
            {
                var newPanel = AddNewPanel();
                newPanel.Initialise(c, this);
            }
        }

        public override void PlayShowAnimation()
        {
            ResetAnimations();

            base.PlayShowAnimation();

            StartAnimations();
        }

        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation();

            backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(0, -1500), 0.3f).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
            {
                UIController.OnPageClosed(this);
            });
        }

        protected override void HidePage(SimpleCallback onFinish)
        {
            UIController.HidePage<UICharactersPanel>(onFinish);
        }

        public override CharacterPanelUI GetPanel(CharacterType characterType) 
            => itemPanels.FirstOrDefault(t => t.Character.Type == characterType);

        #endregion
    }
}