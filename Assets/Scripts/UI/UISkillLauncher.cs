using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UISkillLauncher : SingletonMono<UISkillLauncher>
{
    private Animator _animator;
    public Transform SkillContainer;


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInChildren<Animator>();
    }

    public void Show(bool show, Skill skill = null)
    {
        _animator.SetBool("Out", show);

        //Destruimos el icono previo
        if (SkillContainer.childCount > 0)
            Destroy(SkillContainer.GetChild(0).gameObject);

        //Mostramos el icono de la habilidad lanzada
        if(show && skill)
            Instantiate(UIManager.self.skillsIcons[skill.Type], SkillContainer);

        //TODO: Mostrar/Ocultar contorno de puntos en folio
    }
}
