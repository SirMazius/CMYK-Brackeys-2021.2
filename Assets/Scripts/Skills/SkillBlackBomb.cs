using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

//Habilidad de eliminar un color selectivamente en una zona amplia
public class SkillBlackBomb : Skill
{
    public const float radius = 5f;

    public override void LaunchSkill(Vector3 point)
    {
        base.LaunchSkill(point);
        //TODO: Eliminar negros selectivamente en area
    }
}
