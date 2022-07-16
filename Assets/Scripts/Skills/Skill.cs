using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameGlobals;

public abstract class Skill: MonoBehaviour
{
    public SkillType type;
    public Image icon;

    private bool _grabbing = false;
    public bool Grabbing
    {
        get => _grabbing;
        set
        {
            _grabbing = value;
            //Se cambia la visibilidad del icono de la skill
            icon.enabled = value;
        }
    }


    public void Awake()
    {
        icon = GetComponentInChildren<Image>();
        if (!icon)
            Debug.LogError("Skill sin sprite");
    }


    public void Update()
    {
        //Si se hace click sobre el escenario (no UI) se comienza a atraer moninkers
        if (Input.GetMouseButtonDown(0) && !InputModule.OveredUIElement)
            StartGrabbing();
        //Si se esta agarrando o atrayendo moninkers controlamos el drag y el drop
        else if (Grabbing)
        {
            if (Input.GetMouseButton(0))
                DragGrabbing();
            else if (Input.GetMouseButtonUp(0))
                EndGrabbing();
        }
    }

    public abstract void Launch();

    
    public void StartGrabbing()
    {
        Grabbing = true;
    }

    protected void DragGrabbing()
    {
        transform.position = GameGlobals.Cursor;
    }

    protected void EndGrabbing()
    {
        Grabbing = false;
        Launch();
    }
}