using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillExchanger : MonoBehaviour
{
    private Animator _animator;
    public Image Background;
    public GameObject SkillContainer;
    public static float noSkillAlpha = 0.3f;


    // Start is called before the first frame update
    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void UpdateExchangeBox(bool enough, bool hasSkills)
    {
        bool aux = (enough);
        _animator.SetBool("Out", aux);

        var color = Background.color;
        color.a = hasSkills ? 1f : noSkillAlpha;
        Background.color = color;
    }

    public void AddSkillIcon()
    {
        //TODO:
    }

    public void RemoveSkillIcon()
    {
        //TODO:
    }
}
