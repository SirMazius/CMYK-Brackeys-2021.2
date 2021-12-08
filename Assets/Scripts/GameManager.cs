using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class GameManager : MonoBehaviour
{
    public static GameManager self;
    public static UIManager ui;
    public GameObject moninkerPrefab;
    public GameObject paintShotPrefab, eraserPrefab;
    public Transform floor;
    public BoxCollider floorColl;

    //Pooling de moninkers
    public Queue<MoninkerController> moninkersPool;
    public int poolSize = 1000;
    public int maxFrameSpawn = 2;
    public int currFrameSpawn = 0;

    //Posiciones moninkers iniciales
    public List<Transform> cyanInitPos, magentaInitPos, yellowInitPos;

    //Dimensiones de cuadrados del terreno y listas de moninkers
    public Vector3 gridCorner;
    public int gridX = 15, gridZ = 15;
    public float distX, distZ;
    public List<MoninkerController>[,] gridLists;

    //Puntuacion
    public int[] primariesCount = {0,0,0};
    public int score = 0;

    //Juego y dificultad
    [SerializeField]
    public bool inGame = false;
    public float[,] paintDifficulty = // [niveles] [props(primario,secundario,negro)]
    {
        {1, 0, 0},
        {0.85f, 0.15f, 0},
        {0.7f, 0.3f, 0},
        {0.55f, 0.4f, 0.05f},
        {0.35f, 0.5f, 0.15f}
    };
    private float currGameTime;
    public float secsUntilHardest = 180;

    //Disparos de pintura
    public float maxPaintOffset = 5f, minPaintOffset = 0.5f;
    private float nextPaintOffset;

    //Borrador
    public bool pressed = false;
    public float eraserCooldown = 10;
    private float currEraserCooldown = 0;
    private Vector3 lastEraserPos;
    public PaintShotController currEraser = null;


    void Awake()
    {
        //Singleton
        if (self == null)
            self = this;
        else
            Destroy(gameObject);

        ui = GetComponent<UIManager>();
        ui.InitEraser(eraserCooldown);
    }

    private void Start()
    {
        currGameTime = 0;
        inGame = true;

        //Llenar pool de moninkers
        moninkersPool = new Queue<MoninkerController>();
        for(int i = 0; i< poolSize; i++)
        {
            GameObject go = Instantiate(moninkerPrefab);
            moninkersPool.Enqueue(go.GetComponent<MoninkerController>());
            go.SetActive(false);
        }

        //Preparar grid de listas de moninkers
        gridCorner = floorColl.bounds.min;
        gridCorner.y = 0;
        distX = floorColl.bounds.size.x/gridX;
        distZ = floorColl.bounds.size.z/gridZ;
        gridLists = new List<MoninkerController>[gridX, gridZ];
        for(int i = 0; i<gridX; i++) {
            for (int j = 0; j < gridZ; j++) {
                gridLists[i, j] = new List<MoninkerController>();
            }
        }

                //Colocar moninkers iniciales
                foreach (Transform t in cyanInitPos)
            ActivateMoninker(t.position, InkColorIndex.CYAN);
        foreach (Transform t in magentaInitPos)
            ActivateMoninker(t.position, InkColorIndex.MAGENTA);
        foreach (Transform t in yellowInitPos)
            ActivateMoninker(t.position, InkColorIndex.YELLOW);

        //Preparar putuacion inicial
        score = 0;
        ui.UpdateScore(score);

        //Tippex disponible desde el primer momento
        currEraserCooldown = eraserCooldown;
        StartCoroutine(SpawnPaint());
    }

    void Update()
    {
        //Contadores de tiempo de partida y borrador
        currGameTime += Time.deltaTime;
        currEraserCooldown += Time.deltaTime;

        //Reset limite de spawns por frame
        currFrameSpawn = 0;

        //Pulsar para mostrar area de borrado
        if(ui.UpdateEraser(currEraserCooldown) && !pressed && Input.GetMouseButtonDown(1))
        {
            pressed = true;
            CreateEraser();
        }
        //Soltar clic para disparar borrado
        else if (pressed && !Input.GetMouseButton(1))
        {
            Erase();
            pressed = false;
        }

        //Si se está apuntando con el borrador, cambiamos su posición a lo largo del tablero
        if(currEraser != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 6))
            {
                lastEraserPos = hit.point + Vector3.up*0.0001f;
                currEraser.transform.position = lastEraserPos;
            }
        }
    }

    //Sacar monigote de la cola y cambiar de color
    public void ActivateMoninker(Vector3 pos, InkColorIndex color)
    {
        if (moninkersPool.Count > 0)
        {
            MoninkerController m = moninkersPool.Dequeue();
            m.gameObject.SetActive(true);
            m.transform.position = pos;
            m.SetColor(color);
            AddChildScore();

            //Actualizar contador de primarios
            //if((int)color < 3 && (int)color >= 0)
            //    ui.UpdatePrimary(color, ++primariesCount[(int)color]);
        }
        else
            Debug.Log("ERROR: Cola sin elementos");
    }

    //Desactivar monigote y devolverlo a la pool
    public void DeactivateMoninker(MoninkerController m)
    {
        m.gameObject.SetActive(false);
        moninkersPool.Enqueue(m);
    }

    //Instanciamos el reticulo de borrador sin llegar a lanzarlo
    public void CreateEraser()
    {
        currEraser = Instantiate(eraserPrefab).GetComponent<PaintShotController>();
        currEraser.SetPaintColor(InkColorIndex.NONE);
        currEraserCooldown = 0;
    }

    //Cuando soltamos clic dejamos caer el tipex y ya no lo movemos de posicion
    public void Erase()
    {
        currEraser.Drop();
        currEraser = null;
    }

    //Generador de una bola de pintura con color aleatorio segun la dificultad progresiva
    public IEnumerator SpawnPaint()
    {
        while(inGame)
        {
            //Proporcion de tiempo hasta el tope de dificultad
            float prop = 1 - Mathf.Min(currGameTime/secsUntilHardest, 1); 

            //Calculo de tiempo hasta el siguiente lanzamiento
            nextPaintOffset = (maxPaintOffset-minPaintOffset) * prop + minPaintOffset;

            yield return new WaitForSeconds(nextPaintOffset);

            //Disparo en posicion aleatoria
            GameObject shot = Instantiate(paintShotPrefab);
            PaintShotController shotController = shot.GetComponent<PaintShotController>();
            Vector3 limits = floorColl.bounds.extents;
            Vector3 randPos = new Vector3(Random.Range(-limits.x, limits.x), 0.001f, Random.Range(-limits.z, limits.z));
            shot.transform.position = randPos;

            //Color varia en funcion de la dificultad (mas secundarios y negros conforme avanza el tiempo)
            shotController.SetPaintColor(GetPaintColor(prop));
        }

        yield return null;
    }

    //Metodo para obtener un color aleatorio siguiendo las proporciones de la matriz de dificultad
    public InkColorIndex GetPaintColor(float prop)
    {
        int levels = paintDifficulty.GetLength(0)-1;
        int level = (int)Mathf.Floor((1-prop)*levels);
        
        float random = Random.Range(0f,1f);
        float aux = 0;
        int group = 0;

        for(; group< 3; ++group)
        {
            aux += paintDifficulty[level,group];
            if (random < aux)
                break;
        }

        //Debug.Log("Tirando pintura (nivel " + level + ")");

        switch(group)
        {
            //Color primario aleatorio
            case 0:
                return (InkColorIndex) Random.Range(0, 3);
            //Color secundario aleatorio
            case 1:
                return (InkColorIndex)Random.Range(3, 7);
            //Negro
            default:
                return InkColorIndex.BLACK;
        }
    }

    //Actualizamos contador de primarios y añadimos puntuacion
    public void AddColor(InkColorIndex color)
    {
        if ((int)color >= 0 && (int)color < 3)
        {
            ui.UpdatePrimary(color, ++primariesCount[(int)color]);
            Debug.Log("Add " + color);
        }
    }

    public void AddChildScore()
    {
        ui.UpdateScore(++score);
    }

    public void RemoveColor(InkColorIndex color)
    {
        if ((int)color >= 0 && (int)color < 3)
        {
            if (--primariesCount[(int)color] <= 0)
            { 
                Lose(color);
            }
            ui.UpdatePrimary(color, primariesCount[(int)color]);
            Debug.Log("Remove " + color);
        }
    }


    public void Lose(InkColorIndex color)
    {
        if(inGame)
        {
            inGame = false;
            Debug.Log("YOU LOSE, final score: " + score);
            ui.Lose(color,score);
        }
    }
}
