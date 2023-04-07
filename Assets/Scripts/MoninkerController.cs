using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static GameGlobals;
using Sirenix.OdinInspector;
using DG.Tweening;

public class MoninkerController : SerializedMonoBehaviour
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
    //[SerializeField]
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

                //Ponemos particulas negras al contagiar unos a otros
                if(_moninkerColor != InkColorIndex.BLACK && value == InkColorIndex.BLACK)
                {
                    spawnParticles.Play();
                    AudioManager.self.PlayAdditively(SoundId.Black_contagion);
                    spawnParticlesRend.material.color = UIManager.self.InkColors[InkColorIndex.BLACK];
                }

                _moninkerColor = value;
                GameManager.self.AddColorCount(value);


                mainSpriteRender.sprite = UIManager.self.MoninkerIdleSprite;
                mainSpriteRender.material.SetColor("TintColor", UIManager.self.InkColors[MoninkerColor]);

                //Reseteamos tiempo de heat al teñir
                if (wanderState != null)
                    wanderState.InterruptHeat();


                Heat = false;

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
                    gameObject.layer = layerBlackMoninker;
                }
                else
                {
                    mainSpriteRender.sprite = UIManager.self.MoninkerIdleSprite;
                    gameObject.layer = layerMoninker;
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
    public MoninkerController currTarget;
    [HideInPlayMode]
    public bool reproducing = false;
    public Vector3 grabOffset = new Vector3();
    //Maquina de estados
    [SerializeField]
    public MoninkerState currState;
    [HideInInspector]
    public MoninkerWanderState wanderState;
    [HideInInspector]
    public MoninkerPursueState pursueState;
    [HideInInspector]
    public MoninkerDraggingState draggingState;
    public bool grabbed = false;
    //Colliders moninkers
    public static float normalCollRadius = 0.2f;
    public static float grabbedCollRadius = 0.01f;

    [Header("Reproduction")]
    public MoninkerController ReproductionCompanion = null;
    public Coroutine ReproductionCoroutine = null;
    private bool _heat = false;
    public bool Heat
    {
        get => _heat;
        set {
            _heat = value;
            if(_heat && MoninkerColor != InkColorIndex.BLACK)
                mainSpriteRender.sprite = UIManager.self.MoninkerHeatSprite;
        }
    }

    [Header("Effects")]
    public SpriteRenderer blackWaveRenderer;
    private Sequence blackWaveTween;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        coll = GetComponentInChildren<SphereCollider>(true);
    }

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

        //Billboard sprite
        Vector3 pos = Camera.main.transform.position;
        pos.x = transform.position.x;
        spritesParent.transform.LookAt(pos, Camera.main.transform.up);

        //Negros no se tienen que detectar, se excluyen de las celdas
        if(!grabbed)
        {
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
        if(ReproductionCoroutine == null)
        {
            InkColorIndex childColor = MixColors(MoninkerColor, other.MoninkerColor);
            ReproductionCoroutine = StartCoroutine(ReproduceCoroutine(other, childColor));
            other.ReproductionCoroutine = ReproductionCoroutine;
        }
    }

    public IEnumerator ReproduceCoroutine(MoninkerController other, InkColorIndex childColor)
    {
        reproducing = true;
        other.reproducing = true;

        ReproductionCompanion = other;
        other.ReproductionCompanion = this;
        other.currTarget = this;

        mainSpriteRender.sprite = UIManager.self.MoninkerHeatSprite;
        other.mainSpriteRender.sprite = UIManager.self.MoninkerHeatSprite;
        contourSpriteRender.enabled = true;
        other.contourSpriteRender.enabled = true;

        //Mostrar contorno del color del hijo
        float time = 0;
        Color fillColor = Color.white;
        contourSpriteRender.material.SetColor("ContourColor", UIManager.self.InkColors[childColor]);
        other.contourSpriteRender.material.SetColor("ContourColor", UIManager.self.InkColors[childColor]);

        //Hacer blanco progresivamente
        do{
            if (!reproducing)
            {
                EndReproduction();
                yield break;
            }

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

        //REPRODUCCION COMPLETADA
        EndReproduction(true);
        currState.StartWander();
    }

    public void EndReproduction(bool reproductionCompleted = false)
    {
        reproducing = false;
        Heat = false;
        contourSpriteRender.enabled = false;
        currTarget = null;
        mainSpriteRender.sprite = UIManager.self.MoninkerIdleSprite;
        ReproductionCoroutine = null;

        //Reseteamos heat solo si está completa la reproduccion, si no le ponemos unos segundos de delay
        if (reproductionCompleted)
            wanderState.currHeatTime = 0;
        else
            wanderState.InterruptHeat();

        //Cortamos reproduccion del compañero tambien (evitando bucle infinito)
        if (ReproductionCompanion)
        {
            ReproductionCompanion.ReproductionCompanion = null;
            ReproductionCompanion.EndReproduction(reproductionCompleted);
            ReproductionCompanion = null;
        }
    }

    public void DoBlackWave()
    {
        if (blackWaveTween != null && blackWaveTween.IsPlaying())
            blackWaveTween.Kill();

        blackWaveTween = DOTween.Sequence();

        blackWaveTween
            .Append(blackWaveRenderer.transform.DOScale(UIManager.self.BlackWaveMaxScale, UIManager.self.BlackWaveDuration + 0.1f).ChangeStartValue(Vector3.zero).SetEase(Ease.OutCirc))
            .Insert(0, blackWaveRenderer.DOColor(new Color(0, 0, 0, 0), UIManager.self.BlackWaveDuration-0.1f).ChangeStartValue(new Color(0, 0, 0, 1)).SetEase(Ease.OutCirc));

        AudioManager.self.PlayAdditively(SoundId.First_black);
    }

    //Al tocar la bomba negra, se eliminan los moninkers negros
    public void OnTriggerEnter(Collider other)
    {
        if (/*MoninkerColor == InkColorIndex.BLACK &&*/ other.gameObject.layer == LayerMask.NameToLayer("BlackBomb"))
        {
            GameManager.self.DeactivateMoninker(this);
        }
    }
}
