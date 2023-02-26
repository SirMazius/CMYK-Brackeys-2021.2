using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;
using Sirenix.OdinInspector;

[HideReferenceObjectPicker]
public class ProgressionTimeRange
{
    public string Name;

    [Space][HorizontalGroup("duration",LabelWidth = 80)]
    public float startTime;
    [Space][HorizontalGroup("duration")]
    public float duration = 60;

    public float endTime
    {
        get => startTime + duration;
    }
    //Duración sin contar el tiempo del ultimo disparo (para evitar que en el inicio y fin de los rangos se superpongan lanzamientos)
    public float realDuration 
    { 
        get => duration - spawnCooldownRange.y; 
    }

    //Tiempo en el que se lanzara el ultimo proyectil del rango
    public float realEndTime
    {
        get => startTime + realDuration;
    }


    [Tooltip("Cooldown que tiene al inicio y final del rango de tiempo")]
    public Vector2 spawnCooldownRange = new Vector2(5, 5);

    [Tooltip("Ráfaga de pintura con probabilidades de spawn")][HideReferenceObjectPicker]
    public List<PaintBurst> paintBursts = new List<PaintBurst>();
}