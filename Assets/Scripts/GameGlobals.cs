using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public static class GameGlobals
{
    public enum InkColorIndex
    {
        NONE = -1,
        CYAN = 0,
        MAGENTA = 1,
        YELLOW = 2,
        RED = 3,
        GREEN = 4,
        BLUE = 5,
        BLACK = 6
    }   
    
    public enum SkillType
    {
        DYE_CYAN = 1<<0,
        DYE_MAGENTA = 1<<1,
        DYE_YELLOW = 1<<2,
        DYE_ERASER = 1<<3,
        BLACK_BOMB = 1<<4
    }
    //Selector exclusivo de flag enums
    //
    //private SkillType _exclusiveSkill = 0;
    //[SerializeField][EnumToggleButtons]
    //public SkillType ExclusiveSkill
    //{
    //    get => _exclusiveSkill;
    //    set {
    //        var prev = _exclusiveSkill;
    //        _exclusiveSkill = value & ~prev;
    //    }
    //}

    public enum ExchangerType
    {
        NONE = -1,
        SIMPLE,
        BETTER
    }

    public static Dictionary<InkColorIndex, Color> InkColors = new Dictionary<InkColorIndex, Color>
    {
        {InkColorIndex.CYAN, new Color32(84,236,255,255)}, // Cyan #54ECFF
        {InkColorIndex.MAGENTA, new Color32(255,99,180,255)}, // Magenta #FF63B4
        {InkColorIndex.YELLOW, new Color32(254,233,70,255)}, // Yellow #FEE946 //new Color32(255,236,92,255),// Yellow #FFEC5C 
        {InkColorIndex.RED, new Color32(255,141,92,255)}, // Red #FF8D5C
        {InkColorIndex.GREEN, new Color32(140,255,84,255)}, // Green #8CFF54
        {InkColorIndex.BLUE, new Color32(99,110,255,255)}, // Blue #636EFF
        {InkColorIndex.BLACK, new Color32(48,48,48,255)}, // Black #303030
        {InkColorIndex.NONE, new Color32(180,180,180,255)} // Borrador #C9C9C9
    };

    public static Dictionary<ExchangerType, int> ExchangeQuantities = new Dictionary<ExchangerType, int>()
    {
        {ExchangerType.SIMPLE, 10},
        {ExchangerType.BETTER, 20}
    };

    public const string animPressedParam = "Pressed";
    public static string tagMoninker = "Moninker";

    public static Vector3 Cursor { get => GetCursor3DPoint(); }

    public enum CurveType
    {
        Lineal,
        Smoothed,
        ParabolicLow, //parabola con asintota en Y = 0
        //ParabolicHigh //TODO: parabola con asintota en Y = max
    }


    //Modificacion del input module para poder obtener el objeto de UI sobre el que esta en todo momento
    //https://stackoverflow.com/questions/39150165/how-do-i-find-which-object-is-eventsystem-current-ispointerovergameobject-detect/47412060#47412060
    private static StandaloneInputModuleV2 _inputModule;
    public static StandaloneInputModuleV2 InputModule
    {
        get
        {
            if (Application.isPlaying)
            {
                if(EventSystem.current && EventSystem.current.currentInputModule)
                    _inputModule = EventSystem.current.currentInputModule as StandaloneInputModuleV2;
                if (_inputModule == null)
                    Debug.LogError("Missing StandaloneInputModuleV2");
            }

            return _inputModule;
        }
    }


    #region FUNCIONES

    //Hacemos un raycast para obtener el punto del folio en el que choca
    public static Vector3 GetCursor3DPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 6))
            return hit.point + Vector3.up * 0.0001f;
        else
            return Vector3.positiveInfinity;
    }

    //Devuelve el resultado de mezclar dos colores de moninkers
    public static InkColorIndex MixColors(InkColorIndex c1, InkColorIndex c2)
    {
        InkColorIndex result = InkColorIndex.NONE;

        //Si uno de los dos es negro, contagia al otro y no crea hijo
        if (c1 == InkColorIndex.BLACK || c2 == InkColorIndex.BLACK)
        {
            result = InkColorIndex.BLACK;
        }
        //Mismo color
        else if (c2 == c1)
            result = c1;
        //Mezclas
        else
        {
            //Colores primarios + primarios
            if ((c1 == InkColorIndex.MAGENTA && c2 == InkColorIndex.YELLOW) ||
                (c2 == InkColorIndex.MAGENTA && c1 == InkColorIndex.YELLOW))
                result = InkColorIndex.RED;
            else if ((c1 == InkColorIndex.MAGENTA && c2 == InkColorIndex.CYAN) ||
                (c2 == InkColorIndex.MAGENTA && c1 == InkColorIndex.CYAN))
                result = InkColorIndex.BLUE;
            else if ((c1 == InkColorIndex.CYAN && c2 == InkColorIndex.YELLOW) ||
                (c2 == InkColorIndex.CYAN && c1 == InkColorIndex.YELLOW))
                result = InkColorIndex.GREEN;

            //Secundario + primario ya incluido = secundario
            else if ((c1 == InkColorIndex.RED && (c2 == InkColorIndex.YELLOW || c2 == InkColorIndex.MAGENTA)) || (c2 == InkColorIndex.RED && (c1 == InkColorIndex.YELLOW || c1 == InkColorIndex.MAGENTA)))
                result = InkColorIndex.RED;
            else if ((c1 == InkColorIndex.BLUE && (c2 == InkColorIndex.CYAN || c2 == InkColorIndex.MAGENTA)) || (c2 == InkColorIndex.BLUE && (c1 == InkColorIndex.CYAN || c1 == InkColorIndex.MAGENTA)))
                result = InkColorIndex.BLUE;
            else if ((c1 == InkColorIndex.GREEN && (c2 == InkColorIndex.CYAN || c2 == InkColorIndex.YELLOW)) || (c2 == InkColorIndex.GREEN && (c1 == InkColorIndex.CYAN || c1 == InkColorIndex.YELLOW)))
                result = InkColorIndex.GREEN;

            //(secundario + secundario) / (secundario + primario faltante) = HIJO NEGRO
            else
                result = InkColorIndex.BLACK;
        }

        return result;
    }

    /// <summary>
    /// Devuelve la Y de una parabola concava donde con x (value) maxima Y (return) es 0 y viceversa.
    /// En otras palabras, el valor devuelto va describiendo un descenso a medida que aumenta value, que va decreciendo cada vez en menor medida. Podriamos decir que se hacen asintotas en los ejes X e Y
    ///  .
    ///  .
    ///   . 
    ///     . 
    ///        . 
    ///             .    .    ---> value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <param name="maxReturn"></param>
    /// <returns></returns>
    public static float ParabolicDecrease(float value, float maxValue, float maxReturn)
    {
        return Mathf.Clamp((value*value * (-maxReturn/(maxValue*maxValue)) + maxReturn), 0, maxReturn);
    }

    /// <summary>
    /// Igual que ParabolicDecrease pero al revés: el resultado parte de 0 y va a aumentando cada vez mas hasta alcanzar su máximo cuando el valor tambien es máximo, 
    /// describiendo una parábola ascendente con asintotas en el eje Y y en X = maxValue
    /// 
    ///                   .
    ///                   .
    ///                  . 
    ///                .   
    ///            . 
    ///  .   .         ------> value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <param name="maxReturn"></param>
    /// <returns></returns>
    public static float ParabolicIncrease(float value, float maxValue, float maxReturn)
    {
        return ParabolicDecrease(maxValue - value, maxValue, maxReturn);
    }

    /// <summary>
    /// Cambia progresivamente un valor en el tiempo, siguiendo una función de curva específica.
    /// Hay que devolver el valor de la corrutina por un action, ya que las corrutinas no aceptan atributos por referencia.
    /// </summary>
    /// <param name="outValue"></param>
    /// <param name="original"></param>
    /// <param name="desired"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static IEnumerator ParamTransitionOverTime(Action<float> outValue, float original, float desired, float time, CurveType curve = CurveType.Lineal)
    {
        float currTime = 0;

        //Diferencia del valor actual respecto al deseado (incremento)
        float diff = desired - original;

        while (currTime < time)
        {
            float value = 0;

            //Aumentamos o decrecemos la velocidad siguiendo una parabola hasta alcanzar el valor indicado
            switch (curve)
            {
                //Interpolacion con suavizado en los extremos
                case CurveType.Smoothed:
                    value = Mathf.SmoothStep(original, desired, Mathf.Clamp01(currTime / time));
                    break;
                //Aumento/decremento cada vez mayor
                case CurveType.ParabolicLow:
                    if (diff > 0)
                        value = original + ParabolicIncrease(currTime, time, diff);
                    else
                        value = original - ParabolicIncrease(currTime, time, Mathf.Abs(diff));
                    break;
                //Por defecto interpolacion lineal
                default:
                    value = Mathf.Lerp(original, desired, Mathf.Clamp01(currTime / time));
                    break;
            }

            //Devolver valor cambiado a traves de la action
            outValue(value);

            yield return new WaitForEndOfFrame();
            currTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Metodo de extensión que devuelve convierete un float de segundos a un int de milisegundos
    /// </summary>
    /// <param name="secs"></param>
    /// <returns></returns>
    public static int ToMillis(this float secs)
    {
        return (int)(secs * 1000);
    }

    public static string ToEnumFormat(this string str)
    {
        return str.Trim(' ').Replace(' ', '_');
    }

    public static string FromEnumFormat(this string str)
    {
        return str.Replace('_', ' ');
    }

    public static void CopyFrom(this AudioSource destination, AudioSource original)
    {
        destination.clip = original.clip;
        destination.volume = original.volume;
        destination.pitch = original.pitch;
        destination.loop = original.loop;
        destination.playOnAwake = false;
        destination.spatialBlend = 0; //Sonidos 2D
        destination.outputAudioMixerGroup = original.outputAudioMixerGroup;
    }

    #endregion
}