using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals
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
        DYE_CYAN,
        DYE_MAGENTA,
        DYE_YELLOW,
        RELOAD_ERASER,
        BLACK_BOMB
    }

    public static Dictionary<InkColorIndex, Color> InkColors = new Dictionary<InkColorIndex, Color>
    {
        {InkColorIndex.CYAN, new Color32(84,236,255,255)}, // Cyan #54ECFF
        {InkColorIndex.MAGENTA, new Color32(255,99,180,255)}, // Magenta #FF63B4
        {InkColorIndex.YELLOW, new Color32(254,233,70,255)}, // Yellow #FEE946 //new Color32(255,236,92,255),// Yellow #FFEC5C 
        {InkColorIndex.RED, new Color32(255,141,92,255)}, // Red #FF8D5C
        {InkColorIndex.GREEN, new Color32(140,255,84,255)}, // Green #8CFF54
        {InkColorIndex.BLUE, new Color32(99,110,255,255)}, // Blue #636EFF
        {InkColorIndex.BLACK, new Color32(48,48,48,255)} // Black #303030
    };

    public static Color eraserColor = new Color32(180,180,180,255); //#C9C9C9

    public static string tagMoninker = "Moninker";


    /////////////// FUNCIONES //////////////////

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
}
