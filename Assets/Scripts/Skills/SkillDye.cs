using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;

//Habilidad de bola de pintura para teñir (o borrar) moninkers
public class SkillDye : Skill
{
    private InkColorIndex _color = InkColorIndex.NONE;
    public InkColorIndex Color
    {
        get => _color;
        set
        {
            _color = value;

            switch (_color)
            {
                case InkColorIndex.CYAN:
                    Type = SkillType.DYE_CYAN;
                    break;
                case InkColorIndex.MAGENTA:
                    Type = SkillType.DYE_MAGENTA;
                    break;
                case InkColorIndex.YELLOW:
                    Type = SkillType.DYE_YELLOW;
                    break;
                default:
                    Type = SkillType.DYE_ERASER;
                    break;
            }
        }
    }

    public SkillDye(InkColorIndex color = InkColorIndex.NONE)
    {
        Color = color;
    }


    public override void Launch()
    {
        //Instanciar un chorro de pintura
        PaintSpawner.self.CreatePaintShot(Color, GameGlobals.Cursor);
    }
}
