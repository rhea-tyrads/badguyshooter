using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Watermelon;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

public class TestWorlds : MonoBehaviour
{
    public List<TestWorldsUI> uis = new();
    public GameController game;
    public Canvas canvas;
    public void Hide()
    {
        canvas.enabled = false;
    }

    public void Show()
    {
        canvas.enabled = true;
    }
    void Start()
    {
        Hide();
        foreach (var ui in uis)
        {
            ui.OnClick += Click;
        }
    }

    void Click(TestWorldsUI testWorldsUI)
    {
        var levelSave = SaveController.GetSaveObject<LevelSave>("level");
        levelSave.World = testWorldsUI.id;
        levelSave.Level = 0;
        SaveController.MarkAsSaveIsRequired();
        
        Debug.LogWarning(  "SET: "+levelSave.World);
        foreach (var ui in uis)
            ui.background.color=Color.white;
        
        testWorldsUI.background.color=Color.green;
     //  LevelController.Unload();
        game.InitialiseGame();
        GameLoading.LoadGameScene();
        //SceneManager.LoadScene(0);
    }
}