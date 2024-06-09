using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestWorldsUI : MonoBehaviour
{
    public int id;
    public Button button;
    public event Action<TestWorldsUI> OnClick = delegate { };
    public TextMeshProUGUI txt;
    public Image background;
    void Awake()
    {
        button.onClick.AddListener(Click);
        txt.text = (id + 1).ToString();
        
    }

    void Click()
    {
        OnClick(this);
    }
}