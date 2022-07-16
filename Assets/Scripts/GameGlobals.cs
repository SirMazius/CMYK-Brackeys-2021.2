using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    public static Dictionary<SkillType, Sprite> SkillsIcons = new Dictionary<SkillType, Sprite>();
    public static string tagMoninker = "Moninker";
    

    #region FUNCIONES

    //Hacemos un raycast para obtener el punto del folio en el que choca
    public static Vector3 GetCursorFloorPoint()
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

    //Devuelve la Y de una parabola concava donde con x maxima y es 0 y viceversa
    public static float CenteredInverseParabola(float value, float maxValue, float maxReturn)
    {
        return Mathf.Clamp((value*value * (-maxReturn/(maxValue*maxValue)) + maxReturn), 0, maxReturn);
    }

    #endregion
}
