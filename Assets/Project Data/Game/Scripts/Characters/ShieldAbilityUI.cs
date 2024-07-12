using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldAbilityUI : MonoBehaviour
{
    private Transform player1;
    public Color activeColor;
    public Color passiveColor;
    public List<Image> shields=new();

    public void Set(int amount)
    {
        if (amount > shields.Count) amount = shields.Count;
        
        for (int i = 0; i < amount; i++)
        {
            shields[i].color = activeColor;
        }

        for (int i = amount; i < shields.Count; i++)
        {
            shields[i].color = passiveColor;
        }
    }
    
    
    public void Follow(Transform player)
    {
        player1 = player;
    }
    public void Unparent()
    {
        transform.SetParent(null);
    }
     
    void Update()
    {
        if (player1)
        {
            transform.position = player1.position;
        }
    }
}
