using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

//Habilidad de bola de pintura para teñir moninkers
public class SkillDye : Skill
{
    public InkColorIndex color;

    private void Start()
    {
        switch(color)
        {
            case InkColorIndex.CYAN:
                type = SkillType.DYE_CYAN;
                break;
            case InkColorIndex.MAGENTA:
                type = SkillType.DYE_MAGENTA;
                break;
            case InkColorIndex.YELLOW:
                type = SkillType.DYE_YELLOW;
                break;
        }
    }


    public override void LaunchSkill(Vector3 point)
    {
        //Instanciar un chorro de pintura
        PaintSpawner.self.CreatePaintShot(color, point);
        base.LaunchSkill(point);
    }
}
