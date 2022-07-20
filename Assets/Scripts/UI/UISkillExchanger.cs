using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillExchanger : MonoBehaviour
{
    private Animator _animator;
    private SkillExchanger _exchanger;

    public Image Background;
    public Transform SkillContainer;
    public TextMeshProUGUI ExchangeQuantity;

    public static float noSkillAlpha = 0.5f;


    // Start is called before the first frame update
    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _exchanger = GetComponent<SkillExchanger>();
        ExchangeQuantity.text = _exchanger.ExchangeQuantity.ToString();
    }

    public void UpdateExchangeBox(bool enough, bool hasSkills)
    {
        bool aux = (enough);
        _animator.SetBool("Out", aux);

        var color = Background.color;
        color.a = (hasSkills || enough) ? 1f : noSkillAlpha;
        Background.color = color;
    }

    public void AddSkillIcon(Skill skill)
    {
        skill.ExchangerIcon = Instantiate(UIManager.self.SkillsIcons[skill.Type], SkillContainer);
    }

    public void RemoveSkillIcon(Skill skill)
    {
        Destroy(skill.ExchangerIcon);
    }
}
