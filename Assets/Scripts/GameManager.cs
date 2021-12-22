using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class GameManager : MonoBehaviour
{
    public static GameManager self = null;
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

    //Listas de moninkers y dimensiones de celdas del terreno
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
    public float currGameTime;
    public float secsUntilHardest = 180;

    
    void Awake()
    {
        //Singleton
        if (self == null)
            self = this;
        else
            Destroy(gameObject);
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
        UIManager.self.UpdateScore(score);
    }

    void Update()
    {
        //Contadores de tiempo de partida y borrador
        currGameTime += Time.deltaTime;
        
        //Reset limite de spawns de monigotes por frame
        currFrameSpawn = 0;
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

    public Vector2Int GetCell(Vector3 point)
    {
        Vector2Int cell = new Vector2Int();
        Vector3 localDist = point - gridCorner;
        cell.x = Mathf.FloorToInt(localDist.x / distX);
        cell.y = Mathf.FloorToInt(localDist.z / distZ);
        return cell;
    }

    //Devolvemos el moninker más cercano al punto indicado
    public MoninkerController GetNearestMoninker(Vector3 point, float maxDist = Mathf.Infinity)
    {
        MoninkerController nearest = null;
        float nearestDist = Mathf.Infinity;

        Vector2Int cell = GetCell(point);
        Vector2Int[] checkCells = {cell,
            new Vector2Int(cell.x-1, cell.y),
            new Vector2Int(cell.x+1, cell.y),
            new Vector2Int(cell.x, cell.y+1),
            new Vector2Int(cell.x, cell.y-1)
        };

        foreach (Vector2Int c in checkCells)
        {
            if (c.x >= 0 && c.y >= 0 && c.x < gridX && c.y < gridZ && gridLists[c.x, c.y].Count > 0)
            {
                List<MoninkerController> moninkers = gridLists[c.x, c.y];
                for (int i = 0; i<moninkers.Count; i++)                
                {
                    MoninkerController m = moninkers[i];
                    float currDist = Vector3.Distance(m.transform.position, point);
                    if(currDist<maxDist && currDist<nearestDist)
                    {
                        nearestDist = currDist;
                        nearest = m;
                    }
                }
            }
        }

        return nearest;
    }

    public void GetMoninkersInRadius(Vector3 center, float maxDist, out List <MoninkerController> moninkers)
    {
        moninkers = new List<MoninkerController>();
        Vector2Int cell = GetCell(center);
        Vector2Int[] checkCells = {cell,
            new Vector2Int(cell.x-1, cell.y),
            new Vector2Int(cell.x+1, cell.y),
            new Vector2Int(cell.x, cell.y+1),
            new Vector2Int(cell.x, cell.y-1)
        };

        foreach (Vector2Int c in checkCells)
        {
            if (c.x >= 0 && c.y >= 0 && c.x < gridX && c.y < gridZ && gridLists[c.x, c.y].Count > 0)
            {
                List<MoninkerController> cellMoninkers = gridLists[c.x, c.y];
                for (int i = 0; i < cellMoninkers.Count; i++)
                {
                    MoninkerController m = cellMoninkers[i];
                    float currDist = Vector3.Distance(m.transform.position, center);
                    if (currDist < maxDist)
                        moninkers.Add(m);
                }
            }
        }
    }

    //Actualizamos contador de primarios y añadimos puntuacion
    public void AddColor(InkColorIndex color)
    {
        if ((int)color >= 0 && (int)color < 3)
        {
            UIManager.self.UpdatePrimary(color, ++primariesCount[(int)color]);
            Debug.Log("Add " + color);
        }
    }

    public void AddChildScore()
    {
        UIManager.self.UpdateScore(++score);
    }

    public void RemoveColor(InkColorIndex color)
    {
        if ((int)color >= 0 && (int)color < 3)
        {
            if (--primariesCount[(int)color] <= 0)
            { 
                Lose(color);
            }
            UIManager.self.UpdatePrimary(color, primariesCount[(int)color]);
            Debug.Log("Remove " + color);
        }
    }

    public void Lose(InkColorIndex color)
    {
        if(inGame)
        {
            inGame = false;
            Debug.Log("YOU LOSE, final score: " + score);
            UIManager.self.Lose(color,score);
        }
    }
}
