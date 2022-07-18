using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameGlobals;

public abstract class Skill : MonoBehaviour
{
    private SkillType _type;
    public SkillType Type
    {
        get => _type;
        set
        {
            _type = value;
            Icon.sprite = GameManager.self.SkillsIcons[_type];
        }
    }

    private Image _icon;
    public Image Icon
    {   
        get
        {
            if(!_icon)
                _icon = GetComponentInChildren<Image>();

            if (!_icon)
                Debug.LogError("Skill sin sprite");

            return _icon;
        }
    }

    private bool _grabbing = false;
    public bool Grabbing
    {
        get => _grabbing;
        set
        {
            _grabbing = value;
            //Se cambia la visibilidad del icono de la skill
            Icon.enabled = value;
        }
    }



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