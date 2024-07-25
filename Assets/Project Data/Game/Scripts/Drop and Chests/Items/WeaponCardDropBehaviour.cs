using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class WeaponCardDropBehaviour : BaseDropBehaviour
    {
        [SerializeField] Collider triggerRef;
        [SerializeField] Image itemImage;
        [SerializeField] Image backImage;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] GameObject particleObject;
        [SerializeField] List<ParticleSystem> rarityParticles = new();

        public WeaponData Data { get; private set; }
        public WeaponType WeaponType { get; private set; }

        TweenCase[] throwTweenCases;

        public override void Initialise(DropData dropData, float availableToPickDelay = -1, float autoPickDelay = -1, bool ignoreCollector = false)
        {
            this.dropData = dropData;
            this.AvailableToPickDelay = availableToPickDelay;
            this.AutoPickDelay = autoPickDelay;

            isPicked = false;
            particleObject.transform.localScale = Vector3.zero;

            LevelController.OnPlayerExitLevelEvent += AutoPick;
            CharacterBehaviour.Died += ItemDisable;
        }

        public void SetCardData(WeaponType weaponType)
        {
            WeaponType = weaponType;

            Data = WeaponsController.GetWeaponData(weaponType);
            itemImage.sprite = Data.Icon;
            backImage.color = Data.RarityData.MainColor;
            titleText.text = Data.Name;

            for (var i = 0; i < rarityParticles.Count; i++)
            {
                var main = rarityParticles[i].main;
                main.startColor = Data.RarityData.MainColor.SetAlpha(main.startColor.color.a);
            }
        }

        void AutoPick()
        {
            CharacterBehaviour.GetBehaviour().OnItemPicked(this);

            Pick(false);
            LevelController.OnPlayerExitLevelEvent -= AutoPick;
        }

        public override void Drop()
        {

        }

        public override void Throw(Vector3 position, AnimationCurve movemenHorizontalCurve, AnimationCurve movementVerticalCurve, float time)
        {
            throwTweenCases = new TweenCase[2];

            triggerRef.enabled = false;

            throwTweenCases[0] = transform.DOMoveXZ(position.x, position.z, time).SetCurveEasing(movemenHorizontalCurve);
            throwTweenCases[1] = transform.DOMoveY(position.y, time).SetCurveEasing(movementVerticalCurve).OnComplete(delegate
            {
                Tween.DelayedCall(AvailableToPickDelay, () =>
                {
                    triggerRef.enabled = true;
                });

                if (AutoPickDelay != -1f)
                {
                    Tween.DelayedCall(AutoPickDelay, () => Pick());
                }

                particleObject.transform.DOScale(7f, 0.2f).SetEasing(Ease.Type.SineOut);
            });
        }

        public override void Pick(bool moveToPlayer = true)
        {
            LevelController.OnPlayerExitLevelEvent -= AutoPick;

            if (isPicked)
                return;

            isPicked = true;

            // Kill movement tweens
            if (!throwTweenCases.IsNullOrEmpty())
            {
                for (var i = 0; i < throwTweenCases.Length; i++)
                {
                    throwTweenCases[i].KillActive();
                }
            }

            if (moveToPlayer)
            {
                transform.DOMove(CharacterBehaviour.Transform.position.SetY(0.6f), 0.3f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
                {
                    ItemDisable();
                    AudioController.Play(AudioController.Sounds.cardPickUp);
                });
            }
            else
            {
                ItemDisable();
            }
        }

        public void ItemDisable()
        {
            CharacterBehaviour.Died -= ItemDisable;
            gameObject.SetActive(false);
        }
    }
}