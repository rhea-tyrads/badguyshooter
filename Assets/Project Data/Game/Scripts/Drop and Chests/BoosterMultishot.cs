using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

public class BoosterMultishot : ItemDropBehaviour
{
    public override bool IsPickable(CharacterBehaviour characterBehaviour)
    {
        return true;
        return !characterBehaviour.FullHealth;
    }
}
