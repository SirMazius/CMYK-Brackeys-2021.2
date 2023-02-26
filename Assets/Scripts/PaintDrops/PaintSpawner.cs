using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;
using Sirenix.OdinInspector;


public class PaintSpawner : SerializedMonoBehaviour
{
    public struct BurstAndTime
    {
        public PaintBurst burst;
        public float time;

        public BurstAndTime(PaintBurst burst, float time)
        {
            this.burst = burst;
            this.time = time;
        }
    }

    public static PaintSpawner self;
    public GameObject paintShotPrefab;

    [HideReferenceObjectPicker]
    public List<ProgressionTimeRange> progressionRanges = new List<ProgressionTimeRange>();

    public float endProgressionTime;

    private List<BurstAndTime> bursts = new List<BurstAndTime>();

    //Matriz de dificultad de la pintura creada
    //public float[,] paintDifficulty = // [niveles] [props(primario,secundario,negro)]
    //{
    //    {1, 0, 0},
    //    {0.85f, 0.15f, 0},
    //    {0.7f, 0.3f, 0},
    //    {0.55f, 0.4f, 0.05f},
    //    {0.35f, 0.5f, 0.15f}
    //};

    ////Disparos de pintura
    //public float maxPaintOffset = 5f, minPaintOffset = 0.5f;
    //private float nextPaintOffset;


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
    public void PrepareSpawnList()
    {
        //TODO:

        //Para cada rango de tiempo calculamos qué oleadas y cuando se deben lanzar
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

                //Recorremos todas las oleadas de ese rango y elegimos la oleada que corresponda al random
                float i = 0;
                foreach(var burst in range.paintBursts)
                {
                    i += burst.GetCurrentOdd(rangeProportion);

                    //Si es esta oleada, se almacena en la lista con su marca del tiempo en el que se lanzará
                    if (i<rand)
                    {
                        bursts.Add(new BurstAndTime(burst, currTime+range.startTime)); 
                        break;
                    }
                }

                currTime += timeStep;
            }
        }

        //Ordenamos la lista de oleadas de menos a mas tiempo
    }

    public void GetRandomBurst(ProgressionTimeRange range, float rangeProportion)
    {

    }


    public void StartSpawn()
    {
        //StartCoroutine(SpawnPaint());
    }

    //Generador de una bola de pintura con color aleatorio segun la dificultad progresiva
    //public IEnumerator SpawnPaint()
    //{
    //    while (GameManager.self.IsInGame)
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

    //    yield return null;
    //}

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
    public PaintShotController CreatePaintShot(InkColorIndex color, Vector3 point)
    {
        GameObject shot = Instantiate(paintShotPrefab);
        PaintShotController shotController = shot.GetComponent<PaintShotController>();
        shot.transform.position = point;

        //Color varia en funcion de la dificultad (mas secundarios y negros conforme avanza el tiempo)
        shotController.SetPaintColor(color);

        return shotController;
    }
}
