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
public class SkillExchanger : SerializedMonoBehaviour
{
    public const int SkillsLimit = 2;

    private UISkillExchanger _ui;
    private UIElementCursor _cursorController;

    public ExchangerType Type = ExchangerType.NONE;
    public int ExchangeQuantity;
    [SerializeField]
    private Queue<Skill> _skills = new Queue<Skill>();
    
    private int _grabbeds = 0;
    public int Grabbeds
    {
        get => _grabbeds;
        set
        {
            _grabbeds = value;
            UpdateUI();
        }
    }

    //Launching
    private bool _launching = false;
    public static SkillExchanger LaunchingExchanger = null;
    public static bool IsAnyLaunching { get => LaunchingExchanger != null; }

    public bool Launching
    {
        get => _launching;
        set
        {
            //Evitar lanzar cuando hay uno en lanzamiento
            if(value && IsAnyLaunching)
                return;

            _launching = value;
            LaunchingExchanger = value ? this : null;

            if(value)
                UIManager.self.SetLaunchMode(value, _skills.Peek());
            else
                UIManager.self.SetLaunchMode(value);


            //TODO:
            //- Evitar tocar moninkers u otro exchanger en este modo
            //- Start drag de la habilidad
        }
    }

    private static bool _exchangerClickedThisFrame = false;
    public static bool ExchangerClickedThisFrame { 
        get => _exchangerClickedThisFrame;
        set
        {
            if(true)
                CoroutineExecutor.Instance.StartCoroutine(FrameDelayClicked());
        }
    }


    #region METODOS ESTATICOS EXCHANGERS

    public static void QuitLaunchMode()
    {
        LaunchingExchanger.Launching = false;
        AudioManager.self.PlayOverriding(SoundId.Cancel_launch);
    }

    public static void LaunchCurrentExchanger()
    {
        var skill = LaunchingExchanger.RemoveSkill();
        LaunchingExchanger.Launching = false;
        skill.StartGrabbing();
    }

    #endregion


    #region METODOS EXCHANGER

    private void Awake()
    {
        _ui = GetComponent<UISkillExchanger>();
        _cursorController = GetComponentInChildren<UIElementCursor>();
        _cursorController.OnPressed.AddListener(OnClick);
    }

    private void Start()
    {
        GrabberController.self.OnGrabbedsChange.AddListener((count) => Grabbeds = count);
        UpdateUI();
    }

    //Añadir una skill almacenada, quitando la mas antigua si es necesaria
    public bool AddSkill(Skill skill)
    {
        if(skill)
        {
            //Comprueba si se ha alcanzado el tope de skills. Se elimina la mas antigua si es asi
            if(_skills.Count >= SkillsLimit)
            {
                var removed = _skills.Dequeue();
                _ui.RemoveSkillIcon(removed);
            }

            _skills.Enqueue(skill);

            UpdateUI();
            _ui.AddSkillIcon(skill);

            AudioManager.self.PlayAdditively(SoundId.Exchange_skill);

            return true;
        }
        return false;
    }

    //Elimina la mas antigua
    public Skill RemoveSkill()
    {
        var skill = _skills.Dequeue();
        UpdateUI();
        _ui.RemoveSkillIcon(skill);

        Debug.Log("Quitando skill de exchanger: " + skill.Type);
        return skill;
    }

    //Devuelve la skill conseguida en función del tipo de exchanger y los moninkers cogidos
    public bool TryExchange(int nMoninkers, InkColorIndex grabbedsColor = InkColorIndex.NONE)
    {
        //TODO: Comprobar que no se ha excedido el numero de habilidades
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
    public void OnClick()
    {
        if (_skills.Count > 0 && !Launching && !ExchangerClickedThisFrame)
        {
            //RemoveSkill().StartGrabbing();
            Launching = true;
            ExchangerClickedThisFrame = true;
        }
    }

    #endregion


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

    //Saca o mete el cajetin del exchanger en funcion de si hay grabbeds para intercambio o si hay skill
    private void UpdateUI()
    {
        _ui.UpdateExchangeBox(AreGrabbedsEnough(Grabbeds), (_skills.Count > 0));
    }

    private static IEnumerator FrameDelayClicked()
    {
        _exchangerClickedThisFrame = true;
        yield return new WaitForEndOfFrame();
        _exchangerClickedThisFrame = false;
    }

    #endregion
}