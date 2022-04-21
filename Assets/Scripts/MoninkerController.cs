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
    public InkColorIndex color;
    public static float bodyRadius = 0.4f;

    //Drag
    public bool dragging = false;

    //Listas de monigotes cercanos
    public List<MoninkerController> nearMoninkers = new List<MoninkerController>();
    public Vector2Int currCell = new Vector2Int(); 

    //Parametros de movimiento y tiempos de IA
    [Header("IA")]
    [SerializeField]
    public Transform currTarget;
    public static float wanderSpeed = 1, pursueSpeed = 2f, blackSpeed = 3f;
    public static float wanderTargetMinDist = 0.3f, wanderTargetMaxDist = 1f;
    [Range(0, 1)]
    public static float wanderWaitProbability = 0.7f; //Valor random que altera la probabilidad de hacer esperas durante wander
    public static float wanderWaitMinTime = 0.1f, wanderWaitMaxTime = 0.5f;
    public static float wanderMaxReachTime = 2;
    public static float minHeatTime = 4, maxHeatTime = 12, blackHeatTime = 3;
    public Vector3 dragOffset;

    public bool heat = false;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        //Preparar sprite
        foreach(SpriteRenderer s in sprites)
        {
            s.material.SetColor("TintColor", InkColors[color]);
        }

        //Desactivar collider cuerpo hasta que este en celo
        //coll = GetComponent<SphereCollider>();
        //coll.enabled = false;

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

    public void CreateChild(MoninkerController other)
    {
        InkColorIndex childColor;

        //Si uno de los dos es negro, contagia al otro y no crea hijo
        if (color == InkColorIndex.BLACK || other.color == InkColorIndex.BLACK)
        {
            //Actualizar contador colores primarios
            SetColor(InkColorIndex.BLACK);
            other.SetColor(InkColorIndex.BLACK);
            return;
        }
        //Mismo color
        else if(other.color == color)
            childColor = color;
        else
        {
            //Colores primarios + primarios
            if ((color == InkColorIndex.MAGENTA && other.color == InkColorIndex.YELLOW) ||
                (other.color == InkColorIndex.MAGENTA && color == InkColorIndex.YELLOW))
                childColor = InkColorIndex.RED;
            else if ((color == InkColorIndex.MAGENTA && other.color == InkColorIndex.CYAN) ||
                (other.color == InkColorIndex.MAGENTA && color == InkColorIndex.CYAN))
                childColor = InkColorIndex.BLUE;
            else if ((color == InkColorIndex.CYAN && other.color == InkColorIndex.YELLOW) ||
                (other.color == InkColorIndex.CYAN && color == InkColorIndex.YELLOW))
                childColor = InkColorIndex.GREEN;

            //Secundario + primario ya incluido = secundario
            else if ((color == InkColorIndex.RED && (other.color == InkColorIndex.YELLOW || other.color == InkColorIndex.MAGENTA))|| (other.color == InkColorIndex.RED && (color == InkColorIndex.YELLOW || color == InkColorIndex.MAGENTA)))
                childColor = InkColorIndex.RED;
            else if ((color == InkColorIndex.BLUE && (other.color == InkColorIndex.CYAN || other.color == InkColorIndex.MAGENTA)) || (other.color == InkColorIndex.BLUE && (color == InkColorIndex.CYAN || color == InkColorIndex.MAGENTA)))
                childColor = InkColorIndex.BLUE;
            else if ((color == InkColorIndex.GREEN && (other.color == InkColorIndex.CYAN || other.color == InkColorIndex.YELLOW)) || (other.color == InkColorIndex.GREEN && (color == InkColorIndex.CYAN || color == InkColorIndex.YELLOW)))
                childColor = InkColorIndex.GREEN;

            //(secundario + secundario) / (secundario + primario faltante) = HIJO NEGRO
            else
                childColor = InkColorIndex.BLACK;
        }

        //Instanciamos el hijo con el color correcto en mitad de los dos monigotes
        Vector3 pos = (other.transform.position - transform.position)/2f + transform.position;
        GameManager.self.ActivateMoninker(pos, childColor);

        //GameObject child = Instantiate(GameManager.self.moninkerPrefab);
        //child.GetComponent<MoninkerController>().SetColor(childColor);
        //Actualizar numero de moninkers
        //GameManager.self.AddChildScore();
    }

    public void SetColor(InkColorIndex colorIndex)
    {
        GameManager.self.RemoveColor(color);

        //Teñir de un color
        if (colorIndex!= InkColorIndex.NONE)
        {
            color = colorIndex;
            foreach (SpriteRenderer s in sprites)
            {
                s.material.SetColor("TintColor", InkColors[color]);
                //sprite.color = InkColors[color];
            }
            GameManager.self.AddColor(colorIndex);

            //Si es negro cambiamos sus caracteristicas
            if (color == InkColorIndex.BLACK)
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
        {
            GameManager.self.DeactivateMoninker(this);
            //Destroy(gameObject);
        }
    }

    //private void OnMouseDrag()
    //{
    //    if(Input.GetMouseButton(0) && !dragging && color != InkColorIndex.BLACK)
    //    {
    //        currState.StartDragging();
    //        dragging = true;
    //    }
    //}

    //private void OnMouseUp()
    //{
    //    if(Input.GetMouseButtonUp(0) && dragging)
    //    {
    //        currState.StartWander();
    //        dragging = false;
    //    }
    //}
}
