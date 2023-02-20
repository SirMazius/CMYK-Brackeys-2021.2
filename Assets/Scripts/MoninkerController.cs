using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static GameGlobals;

public class MoninkerController : MonoBehaviour
{
    public NavMeshAgent agent;
    public SphereCollider coll;
    public SphereCollider triggerDetector;
    public Rigidbody2D rb;
    public SpriteRenderer mainSpriteRender;
    public SpriteRenderer contourSpriteRender;
    public GameObject spritesParent;

    public ParticleSystem spawnParticles;
    public ParticleSystemRenderer spawnParticlesRend;

    //Apariencia
    [Header("Appearance")]
    private InkColorIndex _moninkerColor = InkColorIndex.NONE;
    [SerializeField]
    public InkColorIndex MoninkerColor
    {
        get => _moninkerColor;
        set
        {
            //Teñir
            if (value != InkColorIndex.NONE)
            {
                if(_moninkerColor != InkColorIndex.NONE)
                    GameManager.self.RemoveColorCount(_moninkerColor);

                _moninkerColor = value;
                GameManager.self.AddColorCount(value);

                mainSpriteRender.material.SetColor("TintColor", UIManager.self.InkColors[MoninkerColor]);

                //Si es negro cambiamos sus caracteristicas
                if (MoninkerColor == InkColorIndex.BLACK)
                {
                    triggerDetector.radius = 10;
                    if (currState != null)
                    {
                        currState.StartWander();
                        wanderState.currHeatTime = 0;
                    }
                    mainSpriteRender.sprite = UIManager.self.EvilMoninkerSprite;
                }
                else
                {
                    mainSpriteRender.sprite = UIManager.self.MoninkerIdleSprite;
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
    [HideInInspector]
    public bool reproducing = false;

    public Vector3 grabOffset = new Vector3();
    private bool _heat = false;
    public bool Heat
    {
        get => _heat;
        set {
            _heat = value;
            if(_heat && MoninkerColor != InkColorIndex.BLACK)
                mainSpriteRender.sprite = UIManager.self.MoninkerHeatSprite;
            //heatIndicator.SetActive(Heat);
        }
    }
    public GameObject heatIndicator;

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

    public bool grabbed = false;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        coll = GetComponentInChildren<SphereCollider>(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        mainSpriteRender.material.SetColor("TintColor", UIManager.self.InkColors[MoninkerColor]);

        //Inicializar maquina de estados
        wanderState = new MoninkerWanderState(this);
        pursueState = new MoninkerPursueState(this);
        draggingState = new MoninkerDraggingState(this);
        currState = wanderState;
    }

    void Update()
    {
        currState.UpdateState();

        if(!grabbed)
        {
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
            EndReproduction();
            other.EndReproduction();
        }
        //Si son colores normales, crean un nuevo hijo entre medias de los dos
        else
            StartCoroutine(ReproduceCoroutine(other, childColor));
    }

    public IEnumerator ReproduceCoroutine(MoninkerController other, InkColorIndex childColor)
    {
        //Pasar ambos moninkers a modo pursue y reproduccion
        reproducing = true;
        other.reproducing = true;
        other.currTarget = transform;
        other.currState.StartPursue();

        //Mostrar contorno del color del hijo
        float time = 0;
        Color fillColor = Color.white;
        contourSpriteRender.enabled = true;
        other.contourSpriteRender.enabled = true;
        contourSpriteRender.material.SetColor("ContourColor", UIManager.self.InkColors[childColor]);
        other.contourSpriteRender.material.SetColor("ContourColor", UIManager.self.InkColors[childColor]);

        //Hacer blanco progresivamente
        do{
            time += Time.deltaTime;
            fillColor.a = Mathf.Clamp01(time/GameManager.self.reproduceTime);
            contourSpriteRender.material.SetColor("FillColor", fillColor);
            other.contourSpriteRender.material.SetColor("FillColor", fillColor);
            yield return new WaitForEndOfFrame();
        }
        while (time < GameManager.self.reproduceTime);

        //Instanciar nuevo moninker despues de transicion
        Vector3 pos = (other.transform.position - transform.position) / 2f + transform.position;
        GameManager.self.ActivateMoninker(pos, childColor);
        GameManager.self.currFrameSpawn++;

        EndReproduction();
        other.EndReproduction();
    }

    public void EndReproduction()
    {
        reproducing = false;
        contourSpriteRender.enabled = false;
        currState.StartWander();
        wanderState.currHeatTime = 0;
    }
}
