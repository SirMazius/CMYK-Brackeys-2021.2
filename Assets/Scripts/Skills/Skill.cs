using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;
using TMPro;
using System;

public abstract class Skill: MonoBehaviour
{
    public SkillType type;
    public bool grabbing = false;

    public abstract void Launch(Vector3 point);


    public void StartGrabbing()
    {
        grabbing = true;
    }
}