using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    public static class Drop
    {
        static List<IDropItem> dropItems = new();
        static DropAnimation[] dropAnimations;

        public static void Initialise(DropableItemSettings dropSettings)
        {
            dropAnimations = dropSettings.DropAnimations;

            var customDropItems = dropSettings.CustomDropItems;
            foreach (var item in customDropItems)
            {
                RegisterDropItem(item);
            }

            // Register currencies drop
            var currencyDropItem = new CurrencyDropItem();
            currencyDropItem.SetCurrencies(CurrenciesController.Currencies);

            RegisterDropItem(currencyDropItem);
        }

        public static void RegisterDropItem(IDropItem dropItem)
        {
#if UNITY_EDITOR
            if (dropItems.Any(item => item.DropItemType == dropItem.DropItemType))
            {
                Debug.LogError($"Drop item with type {dropItem.DropItemType} is already registered!");
                return;
            }
#endif

            dropItems.Add(dropItem);

            dropItem.Initialise();
        }

        public static IDropItem GetDropItem(DropableItemType dropableItemType)
            => dropItems.FirstOrDefault(item => item.DropItemType == dropableItemType);

        public static DropAnimation GetAnimation(DropFallingStyle dropFallingStyle) 
            => dropAnimations.FirstOrDefault(anim => anim.FallStyle == dropFallingStyle);

        public static GameObject Spawn(DropData dropData, Vector3 spawnPosition, Vector3 rotation, DropFallingStyle fallingStyle, float availableToPickDelay = -1f, float autoPickDelay = -1f, bool rewarded = false)
        {
            var dropItem = GetDropItem(dropData.dropType);
            var itemGameObject = dropItem.GetDropObject(dropData);
            var item = itemGameObject.GetComponent<IDropableItem>();
            item.IsRewarded = rewarded;

            var dropAnimation = GetAnimation(fallingStyle);

            itemGameObject.transform.position = spawnPosition + (Random.insideUnitSphere * 0.05f).SetY(dropAnimation.OffsetY);
            itemGameObject.transform.localScale = Vector3.one;
            itemGameObject.transform.eulerAngles = rotation;
            itemGameObject.SetActive(true);

            var targetPosition = spawnPosition.GetRandomPositionAroundObject(dropAnimation.Radius * 0.9f, dropAnimation.Radius * 1.2f).AddToY(0.1f);

            item.Initialise(dropData, availableToPickDelay, autoPickDelay);
            item.Throw(targetPosition, dropAnimation.FallAnimationCurve, dropAnimation.FallYAnimationCurve, dropAnimation.FallTime);

            return itemGameObject;
        }
    }
}