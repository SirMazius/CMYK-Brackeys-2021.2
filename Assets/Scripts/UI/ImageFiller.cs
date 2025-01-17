using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ImageFiller : MonoBehaviour
{
    public Image filledTitle;
    public Vector2 inkFillingRange;
    private const string _fillingShaderParam = "Filling";


    private void Start()
    {
        if(!filledTitle)
            filledTitle = GetComponentInChildren<Image>();

        filledTitle.material.SetFloat(_fillingShaderParam, inkFillingRange.x);
    }

    public Tween StartCompleteFill(float time)
    {
        return filledTitle.material.DOFloat(inkFillingRange.y, _fillingShaderParam, time)
            .ChangeStartValue(inkFillingRange.x)
            .SetEase(Ease.InOutQuad);
    }

    public void StartCompleteDrain(float time)
    {
        FillTransition(time, inkFillingRange.y, inkFillingRange.x);
    }

    public void FillTransition(float time, float origin, float desired)
    {
        filledTitle.enabled = true;
        StartCoroutine(GameGlobals.ParamTransitionOverTime((filling) => { filledTitle.material.SetFloat(_fillingShaderParam, filling); }, origin, desired, time, GameGlobals.CurveType.Smoothed));
    }    
}
