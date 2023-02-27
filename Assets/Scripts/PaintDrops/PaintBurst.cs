using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;

/// <summary>
/// Objeto oleada de caidas de pintura simultáneos. Contiene las caracteristicas para spawnear la piuntura de cierta manera con cierta probabilidad e cierto rangod de tiempo de juego
/// </summary>
[HideReferenceObjectPicker]
public class PaintBurst
{
    public enum ColorPack
    {
        CMY,
        CMYRGB,
        RGB,
        RGBK,
        K,
        CMYK,
        ALL,
    }

    [Tooltip("Probabilidad de spawnear esta rafaga ")]
    [HorizontalGroup("odds", LabelWidth = 80)][MinValue(0)][MaxValue(1)]
    public float startOdds = 1f;
    [HorizontalGroup("odds")][MinValue(0)][MaxValue(1)]
    public float endOdds = 1f;

    [HorizontalGroup(LabelWidth=40)]
    public ColorPack colors;
    [HorizontalGroup]
    public PaintDropSize size;
    [HorizontalGroup]
    public int dropsNumber = 1;
    //public List<InkColorIndex> colors = new List<InkColorIndex>();

    //Devuelve la probabilidad de que salga esta oleada en cierto punto del rango de tiempo de la progresion
    public float GetCurrentOdd(float proportion)
    {
        return Mathf.Lerp(startOdds, endOdds, proportion);
    }

    public InkColorIndex GetRandomColor()
    {
        List<InkColorIndex> colorList;

        switch(colors)
        {
            default:
            case ColorPack.CMY:
                colorList = new List<InkColorIndex>() { InkColorIndex.CYAN, InkColorIndex.MAGENTA, InkColorIndex.YELLOW };
                break;
            case ColorPack.CMYRGB:
                colorList = new List<InkColorIndex>() { InkColorIndex.CYAN, InkColorIndex.MAGENTA, InkColorIndex.YELLOW, InkColorIndex.RED,InkColorIndex.GREEN, InkColorIndex.BLUE};
                break;
            case ColorPack.RGB:
                colorList = new List<InkColorIndex>() { InkColorIndex.RED, InkColorIndex.GREEN, InkColorIndex.BLUE };
                break;
            case ColorPack.RGBK:
                colorList = new List<InkColorIndex>() { InkColorIndex.RED, InkColorIndex.GREEN, InkColorIndex.BLUE, InkColorIndex.BLACK };
                break;
            case ColorPack.K:
                colorList = new List<InkColorIndex>() { InkColorIndex.BLACK };
                break;
            case ColorPack.CMYK:
                colorList = new List<InkColorIndex>() { InkColorIndex.CYAN, InkColorIndex.MAGENTA, InkColorIndex.YELLOW , InkColorIndex.BLACK};
                break;
            case ColorPack.ALL:
                colorList = new List<InkColorIndex>() { InkColorIndex.CYAN, InkColorIndex.MAGENTA, InkColorIndex.YELLOW, InkColorIndex.RED, InkColorIndex.GREEN, InkColorIndex.BLUE, InkColorIndex.BLACK };
                break;
        }

        return colorList[Random.Range(0, colorList.Count)];
    }
}
