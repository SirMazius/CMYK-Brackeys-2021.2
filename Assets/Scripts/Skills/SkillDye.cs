using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;

//Habilidad de bola de pintura para teñir (o borrar) moninkers
public class SkillDye : Skill
{
    public InkColorIndex Color;

    public SkillDye(InkColorIndex color = InkColorIndex.NONE)
    {
        Color = color;
    }

    private void Start()
    {
        switch(Color)
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
            default:
                type = SkillType.DYE_ERASER;
                break;
        }
    }


    public override void Launch(Vector3 point)
    {
        base.Launch(point);

        //Instanciar un chorro de pintura
        PaintSpawner.self.CreatePaintShot(Color, point);
    }
}
