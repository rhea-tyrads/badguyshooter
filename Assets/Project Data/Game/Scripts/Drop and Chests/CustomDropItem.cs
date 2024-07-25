﻿using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CustomDropItem : IDropItem
    {
        [SerializeField] DropableItemType dropableItemType;
        public DropableItemType DropItemType => dropableItemType;

        [SerializeField] GameObject prefab;
        public GameObject DropPrefab => prefab;

        Pool _pool;

        public CustomDropItem(DropableItemType dropableItemType, GameObject prefab)
        {
            this.dropableItemType = dropableItemType;
            this.prefab = prefab;
        }

        public void Initialise()
        {
            _pool = new Pool(new PoolSettings(dropableItemType.ToString(), prefab, 1, true));
        }

        public GameObject GetDropObject(DropData dropData)
        {
            return _pool.Get();
        }

        public void Unload()
        {
            _pool.ReturnToPoolEverything(true);
        }
    }
}