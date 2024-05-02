using CoreUI;
using System;
using UnityEngine.UI;

public class EulaUI : ScreenUI
{
    public Button acceptButton;
    public Button eulaURLButton;
    public event Action OnURL = delegate { };
    public event Action OnAccept = delegate { };

    protected override void Showing()
    {
        acceptButton.onClick.AddListener(Accept);
        eulaURLButton.onClick.AddListener(URL);
    }
    protected override void Hiding()
    {
        acceptButton.onClick.RemoveListener(Accept);
        eulaURLButton.onClick.RemoveListener(URL);
    }


    void Accept() => OnAccept();
    void URL() => OnURL();
}
