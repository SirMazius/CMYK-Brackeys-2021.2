using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;


public class PaintSpawner : SerializedMonoBehaviour
{
    [HideReferenceObjectPicker]
    public struct BurstAndDelay
    {
        [HorizontalGroup][HideLabel]
        public float delay;
        [HorizontalGroup][HideLabel]
        public InkColorIndex color;
        [HorizontalGroup][HideLabel]
        public PaintDropSize size;
        [HorizontalGroup][HideLabel]
        public int drops;
        [HideInInspector]
        public ProgressionTimeRange range;

        public BurstAndDelay(PaintBurst burst, float delay, ProgressionTimeRange range)
        {
            this.delay = delay;
            color = burst.GetRandomColor();
            size = burst.size;
            drops = burst.dropsNumber;
            this.range = range;
        }
    }

    public static PaintSpawner self;
    public GameObject paintShotPrefabS, paintShotPrefabM, paintShotPrefabL;

    [HideReferenceObjectPicker]
    public List<ProgressionTimeRange> progressionRanges = new List<ProgressionTimeRange>();
    public float endProgressionTime;
    [SerializeField]
    private List<BurstAndDelay> bursts = new List<BurstAndDelay>();


    public void Awake()
    {
        //Singleton
        if (self == null)
            self = this;
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        //El tiempo final de la progresion lo obtenemos del rango que termine mas tarde
        foreach(var range in progressionRanges)
        {
            if(range.endTime>endProgressionTime)
                endProgressionTime = range.endTime;
        }
    }

    
    //Calculamos al inicio todas las oleadas que se van a lazar y en que momento se lanzaran. Durante el juego simpemente se leera esta lista y se espawnearan en el tiempo inidicado
    [Button][ExecuteAlways]
    public void PrepareSpawnList()
    {
        if (bursts != null)
            bursts.Clear();
        bursts = new List<BurstAndDelay>();

        //Para cada rango de tiempo calculamos qué oleadas se lanzan y el tiempo que pasará hasta que se lance cada una respecto la anterior
        foreach (var range in progressionRanges)
        {
            float currCooldown = range.spawnCooldownRange.x;
            float currTime = 0;

            //Mientras que no se exceda el tiempo, generamos oleadas dentro de ese rango
            while (currTime <= range.realDuration)
            {
                float rangeProportion = currTime/range.duration;
                float timeStep = Mathf.Lerp(range.spawnCooldownRange.x, range.spawnCooldownRange.y, rangeProportion);
                float rand = Random.Range(0, 1f);
                Debug.Log(rand);

                //Recorremos todas las oleadas de ese rango y elegimos la oleada que corresponda al random
                float i = 0;
                foreach(var burst in range.paintBursts)
                {
                    i += burst.GetCurrentOdd(rangeProportion);

                    //Si es esta oleada, se almacena en la lista con su marca del tiempo en el que se lanzará
                    if (i>rand)
                    {
                        bursts.Add(new BurstAndDelay(burst, timeStep, range));//currTime+range.startTime)); 
                        break;
                    }
                }

                currTime += timeStep;
            }
        }
    }


    public void StartSpawn()
    {
        StartCoroutine(SpawnPaint());
    }

    //Generador de una bola de pintura con color aleatorio segun la dificultad progresiva
    public IEnumerator SpawnPaint()
    {
        foreach(var burst in bursts)
        {
            yield return new WaitForSeconds(burst.delay);

            if (!GameManager.self.IsInGame)
                yield break;

            //Crear el numero de disparos que indique la oleada
            for(int i = 0; i<burst.drops; i++)
            {
                CreatePaintShot(burst).Drop(false);
            }
        }

        //while (GameManager.self.IsInGame)
        //    {
        //        float currTime = GameManager.self.currGameTime;
        //        //Proporcion de tiempo hasta el tope de dificultad
        //        float prop = 1 - Mathf.Min(GameManager.self.currGameTime/endProgressionTime, 1);

        //        //Calculo de tiempo hasta el siguiente lanzamiento
        //        nextPaintOffset = (maxPaintOffset - minPaintOffset) * prop + minPaintOffset;

        //        yield return new WaitForSeconds(nextPaintOffset);

        //        //Disparo en posicion aleatoria
        //        Vector3 limits = GameManager.self.floorColl.bounds.extents;
        //        Vector3 randPos = new Vector3(Random.Range(-limits.x, limits.x), 0.001f, Random.Range(-limits.z, limits.z));
        //        CreatePaintShot(GetPaintColor(prop), randPos).Drop(false);
        //    }
    }

    //Metodo para obtener un color aleatorio siguiendo las proporciones de la matriz de dificultad
    //public InkColorIndex GetPaintColor(float prop)
    //{
    //    int levels = paintDifficulty.GetLength(0) - 1;
    //    int level = (int)Mathf.Floor((1 - prop) * levels);

    //    float random = Random.Range(0f, 1f);
    //    float aux = 0;
    //    int group = 0;

    //    for (; group < 3; ++group)
    //    {
    //        aux += paintDifficulty[level, group];
    //        if (random < aux)
    //            break;
    //    }

    //    //Debug.Log("Tirando pintura (nivel " + level + ")");

    //    switch (group)
    //    {
    //        //Color primario aleatorio
    //        case 0:
    //            return (InkColorIndex)Random.Range(0, 3);
    //        //Color secundario aleatorio
    //        case 1:
    //            return (InkColorIndex)Random.Range(3, 7);
    //        //Negro
    //        default:
    //            return InkColorIndex.BLACK;
    //    }
    //}

    //Instanciamos un chorro de pintura de un color en una ubicacion concreta
    public PaintShotController CreatePaintShot(BurstAndDelay burst)
    {
        var limits = GameManager.self.floorLimits;
        Vector3 randPos = new Vector3(Random.Range(-limits.x, limits.x), 0.001f, Random.Range(-limits.z, limits.z));

        GameObject shot;
        switch (burst.size)
        {
            default:
            case PaintDropSize.Small:
                shot = Instantiate(paintShotPrefabS);
                break;
            case PaintDropSize.Medium:
                shot = Instantiate(paintShotPrefabM);
                break;
            case PaintDropSize.Large:
                shot = Instantiate(paintShotPrefabL);
                break;
        }

        PaintShotController shotController = shot.GetComponent<PaintShotController>();
        shot.transform.position = randPos;

        //Color varia en funcion de la dificultad (mas secundarios y negros conforme avanza el tiempo)
        shotController.SetPaintColor(burst.color);
        //TODO: cambiar tamaño

        return shotController;
    }

    public PaintShotController CreatePaintShot(InkColorIndex color, Vector3 point)
    {
        GameObject shot = Instantiate(paintShotPrefabM);
        PaintShotController shotController = shot.GetComponent<PaintShotController>();
        shot.transform.position = point;

        //Color varia en funcion de la dificultad (mas secundarios y negros conforme avanza el tiempo)
        shotController.SetPaintColor(color);

        return shotController;
    }
}
