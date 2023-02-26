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
}
