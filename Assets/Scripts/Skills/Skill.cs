using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SkillsController;
using static Globals;
using TMPro;
using System;

public class Skill: MonoBehaviour
{
    public Action<SkillType> OnLaunch;
    public SkillType type;

    [Header("UI")]
    public GameObject iconUI;


    public virtual void LaunchSkill(Vector3 point)
    {
        OnLaunch(type);
    }
}
