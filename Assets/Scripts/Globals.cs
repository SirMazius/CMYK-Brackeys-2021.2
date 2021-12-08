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

    public static Color[] InkColors = 
    { 
        new Color32(84,236,255,255), // Cyan #54ECFF
        new Color32(255,99,180,255), // Magenta #FF63B4
        new Color32(254,233,70,255), // Yellow #FEE946 //new Color32(255,236,92,255),// Yellow #FFEC5C 
        new Color32(255,141,92,255), // Red #FF8D5C
        new Color32(140,255,84,255), // Green #8CFF54
        new Color32(99,110,255,255), // Blue #636EFF
        new Color32(48,48,48,255) // Black #303030
    };

    public static Color eraserColor = new Color32(180,180,180,255); //#C9C9C9

    public static string tagMoninker = "Moninker";
}
