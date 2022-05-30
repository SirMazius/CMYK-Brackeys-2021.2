using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Globals;

public class MoninkerController : MonoBehaviour
{
    public NavMeshAgent agent;
    public SphereCollider coll;
    public SphereCollider triggerDetector;
    public Rigidbody2D rb;
    public SpriteRenderer[] sprites;
    public GameObject spritesParent;

    //Apariencia
    [Header("Appearance")]
    private InkColorIndex _moninkerColor;
    [SerializeField]
    public InkColorIndex MoninkerColor
    {
        get => _moninkerColor;
        set
        {
            GameManager.self.RemoveColorCount(_moninkerColor);

            //Teñir
            if (value != InkColorIndex.NONE)
            {
                _moninkerColor = value;
                GameManager.self.AddColorCount(value);
                foreach (SpriteRenderer s in sprites)
                {
                    s.material.SetColor("TintColor", InkColors[MoninkerColor]);
                }

                //Si es negro cambiamos sus caracteristicas
                if (MoninkerColor == InkColorIndex.BLACK)
                {
                    triggerDetector.radius = 10;
                    if (currState != null)
                    {
                        currState.StartWander();
                        wanderState.currHeatTime = 0;
                    }
                }
            }
            //Borrar
            else
                GameManager.self.DeactivateMoninker(this);
        }
    }

    //Listas de monigotes cercanos
    public List<MoninkerController> nearMoninkers = new List<MoninkerController>();
    public Vector2Int currCell = new Vector2Int();

    //Parametros de movimiento y tiempos de IA
    [Header("IA")][SerializeField]
    public Transform currTarget;
    public static float wanderSpeed = 1, pursueSpeed = 2f, blackSpeed = 3f;
    public static float wanderTargetMinDist = 0.3f, wanderTargetMaxDist = 1f;
    [Range(0, 1)]
    public static float wanderWaitProbability = 0.7f; //Valor random que altera la probabilidad de hacer esperas durante wander
    public static float wanderWaitMinTime = 0.1f, wanderWaitMaxTime = 0.5f;
    public static float wanderMaxReachTime = 2;
    public static float minHeatTime = 4, maxHeatTime = 12, blackHeatTime = 3;

    public Vector3 grabOffset = new Vector3();
    public bool heat = false;

    //Colliders moninkers
    public static float normalCollRadius = 0.2f;
    public static float grabbedCollRadius = 0.01f;
    

    //Maquina de estados
    [HideInInspector]
    public MoninkerState currState;
    [HideInInspector]
    public MoninkerWanderState wanderState;
    [HideInInspector]
    public MoninkerPursueState pursueState;
    public MoninkerDraggingState draggingState;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        sprites = GetComponentsInChildren<SpriteRenderer>();
        coll = GetComponentInChildren<SphereCollider>(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        //Preparar sprite
        foreach(SpriteRenderer s in sprites)
        {
            s.material.SetColor("TintColor", InkColors[MoninkerColor]);
        }

        //Inicializar maquina de estados
        wanderState = new MoninkerWanderState(this);
        pursueState = new MoninkerPursueState(this);
        draggingState = new MoninkerDraggingState(this);
        currState = wanderState;
    }

    void Update()
    {
        currState.UpdateState();

        //Billboard sprite
        Vector3 pos = Camera.main.transform.position;
        pos.x = transform.position.x;
        spritesParent.transform.LookAt(pos, Camera.main.transform.up);

        //Detectar la casilla de la grid en la que se coloca
        Vector2Int newCell = GameManager.self.GetCell(transform.position);
        
        //Si se ha cambiado de celda, hacemos el cambio de lista
        if(currCell != newCell)
        {
            GameManager.self.gridLists[currCell.x, currCell.y].Remove(this);
            currCell = newCell;
            GameManager.self.gridLists[currCell.x, currCell.y].Add(this);
        }
    }

    //Crea un hijo con el color mezclado de los dos monigotes (o contyagia si es negro)
    public void ReproduceWith(MoninkerController other)
    {
        InkColorIndex childColor = MixColors(MoninkerColor, other.MoninkerColor);

        //Si uno es negro no se reproduce, sino que contagia al otro
        if(childColor == InkColorIndex.BLACK)
        {
            MoninkerColor = InkColorIndex.BLACK;
            other.MoninkerColor = InkColorIndex.BLACK;
        }
        //Si son colores normales, crean un nuevo hijo entre medias de los dos
        else
        {
            Vector3 pos = (other.transform.position - transform.position)/2f + transform.position;
            GameManager.self.ActivateMoninker(pos, childColor);
        }
    }
}
