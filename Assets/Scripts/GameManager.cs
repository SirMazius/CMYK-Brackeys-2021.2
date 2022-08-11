using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;

public class GameManager : SerializedMonoBehaviour
{
    public enum GameState
    {
        MENU,
        GAME,
        PAUSE,
        ENDGAME
    }

    public static GameManager self = null;

    [Header("Game variables")]
    private GameState _currentState = GameState.MENU;

    public bool IsInGame
    {
        get => (_currentState == GameState.GAME);
    }

    [Header("Prefabs")]
    public GameObject moninkerPrefab;
    public GameObject paintShotPrefab, eraserPrefab;

    [Header("Enviroment")]
    public Transform floor;
    public BoxCollider floorColl;

    [Header("Pooling moninkers")]
    public static Queue<MoninkerController> moninkersPool;
    public int poolSize = 1000;
    public int maxFrameSpawn = 2;
    public int currFrameSpawn = 0;

    [Header("Moninkers y celdas")]
    public Vector3 gridCorner;
    public int gridX = 15, gridZ = 15;
    public float distX, distZ;
    [HideInInspector]
    public List<MoninkerController>[,] gridLists;
    public List<Transform> cyanInitPos, magentaInitPos, yellowInitPos;

    [Header("Puntuacion")]
    public int[] primariesCount = {0,0,0};
    public int score = 0;

    [Header("Juego y dificultad")]
    public float currGameTime;
    public float secsUntilHardest = 180;

    [Header("Skills")]
    public GameObject SkillPrefab;


    private void Awake()
    {
        //Singleton
        if (self == null)
            self = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ShowMenu();
        PrepareGame();
    }

    private void Update()
    {
        //Contadores de tiempo de partida y borrador
        currGameTime += Time.deltaTime;
        
        //Reset limite de spawns de monigotes por frame
        currFrameSpawn = 0;
    }


    #region TRANSICIONES ESTADOS

    public void ShowMenu()
    {
        _currentState = GameState.MENU;
        //TODO: Mostrar menu con opciones y cambiar posicion de camara arriba
        UIManager.self.SetMainMenuShow(true);
    }

    public void PrepareGame()
    {
        currGameTime = 0;

        //Llenar pool de moninkers
        moninkersPool = new Queue<MoninkerController>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(moninkerPrefab);
            moninkersPool.Enqueue(go.GetComponent<MoninkerController>());
            go.SetActive(false);
        }

        //Preparar grid de listas de moninkers
        gridCorner = floorColl.bounds.min;
        gridCorner.y = 0;
        distX = floorColl.bounds.size.x / gridX;
        distZ = floorColl.bounds.size.z / gridZ;
        gridLists = new List<MoninkerController>[gridX, gridZ];
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridZ; j++)
            {
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
    }

    //Activa moninkers, muestra UI in game, comienza lluvia de pintura
    public void StartGame()
    {
        _currentState = GameState.GAME;
        PaintSpawner.self.StartSpawn();
        UIManager.self.StartGameUI();
    }

    public void Pause()
    {
        if (IsInGame)
        {
            _currentState = GameState.MENU;
            //TODO: Mostrar menu y pausar accion
        }
        else
            Debug.LogError("No tiene sentido pausar fuera de juego");
    }

    //Finaliza el juego mostrando resultados y opciones
    public async void EndGame(InkColorIndex color)
    {
        if(IsInGame)
        {
            _currentState = GameState.ENDGAME;
            //Animacion final
            await UIManager.self.LoseUI(color, score);
            //TODO: mostrar opciones reinicio, volver a menus, highscores
            _currentState = GameState.MENU;
        }
    }

    #endregion


    #region POOLING MONINKERS

    //Sacar monigote de la cola y cambiar de color
    public void ActivateMoninker(Vector3 pos, InkColorIndex color)
    {
        if (moninkersPool.Count > 0)
        {
            MoninkerController m = moninkersPool.Dequeue();
            m.gameObject.SetActive(true);
            m.transform.position = pos;
            m.MoninkerColor = color;
            if(_currentState == GameState.GAME)
            {
                AudioManager.self.PlayAdditively(SoundId.New_Moninker);
            }
            AddScore();
        }
        else
            Debug.Log("ERROR: Cola sin elementos");
    }

    //Desactivar monigote y devolverlo a la pool
    public void DeactivateMoninker(MoninkerController m)
    {
        RemoveColorCount(m.MoninkerColor);
        m.gameObject.SetActive(false);
        moninkersPool.Enqueue(m);
    }

    #endregion


    #region SISTEMA CELDAS

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
        List<Vector2Int> checkCells = GetCheckCellsInRadius(center, maxDist);

        foreach (Vector2Int c in checkCells)
        {
            if (c.x >= 0 && c.y >= 0 && c.x < gridX && c.y < gridZ && gridLists[c.x, c.y].Count > 0)
            {
                List<MoninkerController> cellMoninkers = gridLists[c.x, c.y];
                for (int i = 0; i < cellMoninkers.Count; i++)
                {
                    MoninkerController m = cellMoninkers[i];
                    float currDist = Vector3.Distance(m.transform.position, center);
                    if (currDist < maxDist || m.currState is MoninkerDraggingState)
                        moninkers.Add(m);
                }
            }
        }
    }

    //Devuelve una lista de celdas en las que comporbar la cercania desde un punto
    private List<Vector2Int> GetCheckCellsInRadius(Vector3 center, float radius)
    {
        List<Vector2Int> checkCells = new List<Vector2Int>();

        //Obtenemos el numero de celdas de radio que vamos a seleccionar y la celda central de la que partimos
        int radiusCellsX =  Mathf.CeilToInt(radius/distX);
        int radiusCellsY =  Mathf.CeilToInt(radius/distZ);
        Vector2Int centerCell = GetCell(center);

        //Preparamos el array con el rectangulo de celdas en función del radio
        for(int i = Mathf.Max(0, centerCell.x - radiusCellsX); i <= Mathf.Min(centerCell.x + radiusCellsX, gridX); i++)
            for(int j = Mathf.Max(0, centerCell.y - radiusCellsY); j <= Mathf.Min(centerCell.y + radiusCellsY, gridZ); j++)
                checkCells.Add(new Vector2Int(i,j));

        return checkCells;
    }


    #endregion


    #region PUNTUACION Y CONTEO

    //Actualizamos contador de primarios y añadimos puntuacion
    public void AddColorCount(InkColorIndex color)
    {
        if ((int)color >= 0 && (int)color < 3)
            UIManager.self.UpdatePrimary(color, ++primariesCount[(int)color]);
    }

    public void RemoveColorCount(InkColorIndex color)
    {
        if ((int)color >= 0 && (int)color < 3)
        {
            if (--primariesCount[(int)color] <= 0)
                EndGame(color);
            UIManager.self.UpdatePrimary(color, primariesCount[(int)color]);
        }
    }

    public void AddScore()
    {
        UIManager.self.UpdateScore(++score);
    }

    #endregion
}
