using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;
using System.Linq;

//Habilidad de eliminar un color selectivamente en una zona amplia
public class SkillBlackBomb : Skill
{
    public const float radius = 5f;

    public override void Launch()
    {
        var moninkers = FindObjectsOfType<MoninkerController>();
        foreach(var m in moninkers)
        {
            if (m.MoninkerColor == InkColorIndex.BLACK)
                GameManager.self.DeactivateMoninker(m);
        }
    }
}
