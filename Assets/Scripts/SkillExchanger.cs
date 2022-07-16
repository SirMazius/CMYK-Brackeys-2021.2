using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;
using Sirenix.OdinInspector;
using UnityEditor;

//Intercambiador de habilidades que controla tanto la generacion de nuevas habilidades en funcion del tipo
// y moninkers arrastrados a el como el almacenaje de habilidades conseguidas
public class SkillExchanger : SerializedMonoBehaviour
{
    public static int SkillsLimit = 2;

    public ExchangerType Type = ExchangerType.NONE;
    public int ExchangeQuantity;
    private Queue<Skill> _skills = new Queue<Skill>();


    //Añadir una skill almacenada, quitando la mas antigua si es necesaria
    public bool AddSkill(Skill skill)
    {
        if(!skill)
        {
            //Comprueba si se ha alcanzado el tope de skills. Se elimina la mas antigua si es asi
            if(_skills.Count >= SkillsLimit)
                _skills.Dequeue();

            _skills.Enqueue(skill);
            return true;
        }
        return false;
    }

    //Elimina la mas antigua
    public void RemoveSkill()
    {
        _skills.Dequeue();
    }

    //Devuelve la skill conseguida en función del tipo de exchanger y los moninkers cogidos
    public bool TryExchange(int nMoninkers, InkColorIndex grabbedsColor = InkColorIndex.NONE)
    {
        if(AreGrabbedsEnough(nMoninkers))
        {
            Skill newSkill = CreateSkill(grabbedsColor);
            return AddSkill(newSkill);
        }
        else
        {
            Debug.Log("No hay suficientes grabbeds para intercambiar en el exchanger " + Type);
            return false;
        }
    }
    
    //Cuando se pulsa un exchanger se obtiene la skill disponible mas antigua
    public void OnPressExchanger()
    {
        if(_skills.Count > 0)
        {
            _skills.Dequeue().StartGrabbing();
        }
    }


    #region METODOS INTERNOS

    //Devuelve si son suficientes moninkers para intercambiar
    private bool AreGrabbedsEnough(int nMoninkers)
    {
        return Type != ExchangerType.NONE && ExchangeQuantity <= nMoninkers;
    }

    //Devuelve un objeto skill del tipo correspondiente al exchanger y color de moninkers
    private Skill CreateSkill(InkColorIndex color)
    {
        switch (Type)
        {
            case ExchangerType.SIMPLE:
                return new SkillDye(color);
            case ExchangerType.BETTER:
                return new SkillBlackBomb();
            default:
                Debug.Log("Error al crear skill");
                return null;
        }
    }

    //TODO: Controlar hover de exchangers
    //public UIElementCursor SkillPanelSimple, SkillPanelBetter;

    #endregion
}