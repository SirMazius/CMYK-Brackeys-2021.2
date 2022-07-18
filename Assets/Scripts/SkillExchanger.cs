using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Intercambiador de habilidades que controla tanto la generacion de nuevas habilidades en funcion del tipo
// y moninkers arrastrados a el como el almacenaje de habilidades conseguidas
public class SkillExchanger : SerializedMonoBehaviour, IPointerDownHandler
{
    public static int SkillsLimit = 2;

    public ExchangerType Type = ExchangerType.NONE;
    public int ExchangeQuantity;
    [SerializeField]
    private Queue<Skill> _skills = new Queue<Skill>();
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }


    //Añadir una skill almacenada, quitando la mas antigua si es necesaria
    public bool AddSkill(Skill skill)
    {
        if(skill)
        {
            //Comprueba si se ha alcanzado el tope de skills. Se elimina la mas antigua si es asi
            if(_skills.Count >= SkillsLimit)
                _skills.Dequeue();

            _skills.Enqueue(skill);

            UpdateUI();

            return true;
        }
        return false;
    }

    //Elimina la mas antigua
    public Skill RemoveSkill()
    {
        var skill = _skills.Dequeue();
        UpdateUI();
        Debug.Log("Quitando skill de exchanger: " + skill.Type);
        return skill;
    }

    //Devuelve la skill conseguida en función del tipo de exchanger y los moninkers cogidos
    public bool TryExchange(int nMoninkers, InkColorIndex grabbedsColor = InkColorIndex.NONE)
    {
        if(AreGrabbedsEnough(nMoninkers))
        {
            Skill newSkill;
            CreateSkill(grabbedsColor, out newSkill);
            return AddSkill(newSkill);
        }
        else
        {
            Debug.Log("No hay suficientes grabbeds para intercambiar en el exchanger " + Type);
            return false;
        }
    }

    //Cuando se pulsa un exchanger se obtiene la skill disponible mas antigua
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_skills.Count > 0)
        {
            RemoveSkill().StartGrabbing();
        }
    }


    #region METODOS INTERNOS

    //Devuelve si son suficientes moninkers para intercambiar
    private bool AreGrabbedsEnough(int nMoninkers)
    {
        return Type != ExchangerType.NONE && ExchangeQuantity <= nMoninkers;
    }

    //Devuelve un objeto skill del tipo correspondiente al exchanger y color de moninkers
    private void CreateSkill(InkColorIndex color, out Skill skill)
    {
        //TODO: Instanciar el gameobject con el componente skill (prefab)
        switch (Type)
        {
            case ExchangerType.SIMPLE:
                skill = Skill.CreateSkill<SkillDye>();
                (skill as SkillDye).Color = color;
                break;
            case ExchangerType.BETTER:
                skill = Skill.CreateSkill<SkillBlackBomb>();
                break;
            default:
                Debug.Log("Error al crear skill");
                skill = null;
                break;
        }
    }

    //TODO:
    private void UpdateUI()
    {
        var color = _image.color;

        if (_skills.Count > 0)
            color.a = 1;
        else
            color.a = 0.5f;

        _image.color = color;
    }

    #endregion
}