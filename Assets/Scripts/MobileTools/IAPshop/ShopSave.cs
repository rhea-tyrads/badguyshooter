using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopSave : MonoBehaviour
{
    public ShopFile file;
    const string KEY = "SHOP_SAVE";

    void Awake()
    {
        Load();
    }

    public void Add(string id)
    {
        if (IsPurchased(id)) return;
        
        file.purchasedItems.Add(id);
        Save();
    }

    public bool IsPurchased(string id)
        => file.purchasedItems.Any(i => i.Equals(id));

    void Save()
        => PlayerPrefs.SetString(KEY, JsonUtility.ToJson(file));

    void Load() =>
        file = PlayerPrefs.HasKey(KEY)
            ? JsonUtility.FromJson<ShopFile>(PlayerPrefs.GetString(KEY, string.Empty))
            : new ShopFile();
}