using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameGlobals;
using DG.Tweening;


public abstract class Skill : MonoBehaviour
{
    private SkillType _type;
    public SkillType Type
    {
        get => _type;
        set => _type = value;
    }

    private bool _grabbing = false;
    public bool Grabbing
    {
        get => _grabbing;
        set => _grabbing = value;
    }

    public GameObject ExchangerIcon;


    public void Update()
    {
        //Si se esta agarrando o atrayendo moninkers controlamos el drag y el drop
        if (Grabbing)
        {
            if (Input.GetMouseButton(0))
                DragGrabbing();
            else if (Input.GetMouseButtonUp(0))
                EndGrabbing();
        }
    }

    public virtual void Launch()
    {
        Grabbing = false;
    }

    public virtual void StartGrabbing()
    {
        Grabbing = true;
    }

    protected virtual void DragGrabbing()
    {
        transform.position = GameGlobals.Cursor;
    }

    protected virtual void EndGrabbing()
    {
        if(IsCursorHoverFloor)
            Launch();

        Grabbing = false;
    }

    public static T CreateSkill<T>() where T : Skill
    {
        var go = Instantiate(GameManager.self.SkillPrefab);
        T skill = go.AddComponent<T>();
        return skill;
    }
}