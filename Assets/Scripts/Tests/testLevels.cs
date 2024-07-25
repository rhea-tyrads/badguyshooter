using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class testLevels : MonoBehaviour
{
    public int testWorld;

    void Start()
    {
        Invoke(nameof(Cheat), 1f);
    }

    public GameController game;

    void Cheat()
    {
        var save = SaveController.GetSaveObject<LevelSave>("level");
        save.World = testWorld;
        save.Level = 0;
        SaveController.MarkAsSaveIsRequired();
 
        game.InitialiseGame();
        GameLoading.LoadGameScene();
        //  SaveController.Save(true);
    }
}