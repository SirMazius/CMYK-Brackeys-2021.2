using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class BackgroundController : SingletonMono<BackgroundController>
{
    private Renderer rend;
    private float runTime;

    public float normalSpeed = 0.5f;
    public Vector2 speedRange = new Vector2(0, 2f);

    private float _currentSpeed = 0;


    protected override void Awake()
    {
        base.Awake();
        rend = GetComponent<Renderer>();
    }

    //Desplazar fondo en funcion mas rapido en menu que en juego
    private void Update()
    {
        _currentSpeed = Mathf.Clamp(_currentSpeed, speedRange.x, speedRange.y);

        runTime += Time.deltaTime * _currentSpeed;
        rend.material.SetTextureOffset("_BaseMap", new Vector2(runTime, 0));
    }

    //Aparicion de fondo progresiva
    public Tween Show(float opacityIncrTime)
    {
        Color col = rend.material.color;
        col.a = 0;
        return rend.material.DOFade(1, opacityIncrTime).ChangeStartValue(col);
        //OpacityTransition(opacityIncrTime, 1, 0);
    }

    //Desaparicion de fondo progresiva
    public void Hide(float opacityIncrTime)
    {
        OpacityTransition(opacityIncrTime, 0, 1);
    }

    public Tween StartMoving(float speedIncrTime)
    {
        return DOVirtual.Float(0, normalSpeed, speedIncrTime, (speed) => { _currentSpeed = speed; });
        //SpeedTransition(speedIncrTime, normalSpeed, 0);
    }

    public void StartStopping(float speedIncrTime)
    {
        SpeedTransition(speedIncrTime, 0, normalSpeed);
    }


    /// <summary>
    /// Aumenta o desciende de velocidad del fondo progresivamente hasta alcanzar el valor deseado.
    /// </summary>
    /// <param name="time">Tiempo en el que se debe completar la transición</param>
    /// <param name="finalSpeed">Opacidad deseada al final de la transición</param>
    /// <param name="initSpeed">Permite forzar un valor en el que se inicia la transición. Si se deja en -infinito, se toma el valor actual de velocidad</param>
    public void SpeedTransition(float time, float finalSpeed, float initSpeed = Mathf.NegativeInfinity)
    {
        //Por defecto se toma el valor actual de velocidad
        if (initSpeed == Mathf.NegativeInfinity)
            initSpeed = _currentSpeed;

        //Clamp de velocidades maximas y minimas
        initSpeed = Mathf.Clamp(initSpeed, speedRange.x, speedRange.y);
        finalSpeed = Mathf.Clamp(finalSpeed, speedRange.x, speedRange.y);

        //Modificar progresivamente la velocidad según una curva parabólica
        StartCoroutine(GameGlobals.ParamTransitionOverTime((speed) => { _currentSpeed = speed; }, initSpeed, finalSpeed, time, GameGlobals.CurveType.ParabolicLow));
    }


    /// <summary>
    /// Aumenta o desciende de opacidad progresivamente hasta alcanzar el valor deseado.
    /// </summary>
    /// <param name="time">Tiempo en el que se debe completar la transición</param>
    /// <param name="finalOpacity">Opacidad deseada al final de la transición</param>
    /// <param name="initOpacity">Permite forzar un valor en el que se inicia la transición. Si se deja en -infinito, se toma el valor actual de opacidad</param>
    public void OpacityTransition(float time, float finalOpacity, float initOpacity = Mathf.NegativeInfinity)
    {
        //Por defecto se toma el valor actual de opacidad
        if(initOpacity == Mathf.NegativeInfinity)
            initOpacity = rend.material.color.a;

        //Auymentamos la opacidad siguiendo una curva parabolica
        StartCoroutine(GameGlobals.ParamTransitionOverTime(
        (alpha) => {
            Color col = rend.material.color;
            col.a = alpha;
            rend.material.SetColor("_BaseColor", col);
        }, 
        initOpacity, finalOpacity, time, GameGlobals.CurveType.ParabolicLow));
    }
}
