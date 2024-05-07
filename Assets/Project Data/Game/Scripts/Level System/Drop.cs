using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    public static class Drop
    {
        static List<IDropItem> dropItems = new List<IDropItem>();
        static DropAnimation[] dropAnimations;

        public static void Initialise(DropableItemSettings dropSettings)
        {
            dropAnimations = dropSettings.DropAnimations;

            var customDropItems = dropSettings.CustomDropItems;
            for(var i = 0; i < customDropItems.Length; i++)
            {
                RegisterDropItem(customDropItems[i]);
            }

            // Register currencies drop
            var currencyDropItem = new CurrencyDropItem();
            currencyDropItem.SetCurrencies(CurrenciesController.Currencies);

            RegisterDropItem(currencyDropItem);
        }

        public static void RegisterDropItem(IDropItem dropItem)
        {
#if UNITY_EDITOR
            for(var i = 0; i < dropItems.Count; i++)
            {
                if(dropItems[i].DropItemType == dropItem.DropItemType)
                {
                    Debug.LogError(string.Format("Drop item with type {0} is already registered!", dropItem.DropItemType));

                    return;
                }
            }
#endif

            dropItems.Add(dropItem);

            dropItem.Initialise();
        }

        public static IDropItem GetDropItem(DropableItemType dropableItemType)
        {
            for (var i = 0; i < dropItems.Count; i++)
            {
                if (dropItems[i].DropItemType == dropableItemType)
                {
                    return dropItems[i];
                }
            }

            return null;
        }

        public static DropAnimation GetAnimation(DropFallingStyle dropFallingStyle)
        {
            for (var i = 0; i < dropAnimations.Length; i++)
            {
                if (dropAnimations[i].FallStyle == dropFallingStyle)
                {
                    return dropAnimations[i];
                }
            }

            return null;
        }

        public static GameObject DropItem(DropData dropData, Vector3 spawnPosition, Vector3 rotation, DropFallingStyle fallingStyle, float availableToPickDelay = -1f, float autoPickDelay = -1f, bool rewarded = false)
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