using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;

//Habilidad de bola de pintura para teñir (o borrar) moninkers
public class SkillDye : Skill
{
    private InkColorIndex _color = InkColorIndex.NONE;
    private PaintShotController paintShot = null;

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
                    _color = InkColorIndex.NONE;
                    break;
            }
        }
    }

    public SkillDye(InkColorIndex color = InkColorIndex.NONE)
    {
        Color = color;
    }

    //Soltar chorro de pintura
    public override void Launch()
    {
        base.Launch();

        if (paintShot)
        {
            //paintShot.SetPaintColor(Color);
            paintShot.Drop(true);
        }
    }

    public override void StartGrabbing()
    {
        base.StartGrabbing();
        paintShot = PaintSpawner.self.CreatePaintShot(Color, GameGlobals.Cursor);
    }

    protected override void DragGrabbing()
    {
        base.DragGrabbing();
        paintShot.transform.position = GameGlobals.Cursor;
    }
}
