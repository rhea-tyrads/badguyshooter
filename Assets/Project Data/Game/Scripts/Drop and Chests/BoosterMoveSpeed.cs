using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

public class BoosterMoveSpeed : ItemDropBehaviour
{
    public override bool IsPickable(CharacterBehaviour characterBehaviour)
    {
        return true;
    }
}
