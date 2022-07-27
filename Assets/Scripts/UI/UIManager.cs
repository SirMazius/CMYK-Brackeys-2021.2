using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static GameGlobals;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class UIManager : SerializedMonoBehaviour
{
    public static UIManager self;

    [Header("Main menu")]
    [SerializeField]
    private GameObject _mainMenu;
    public Vector2 inkFillingRange;
    public Image filledTitle;
    public float fillingTime = 1.5f;
    private const string _fillinfShaderParam = "Filling";
    private Coroutine startGameCoroutine = null;

    [Header("In game")]
    [SerializeField]
    private GameObject _inGameUI;
    public Dictionary<InkColorIndex,TextMeshProUGUI> primaryCounts = new Dictionary<InkColorIndex, TextMeshProUGUI>();
    public Dictionary<SkillType, GameObject> skillsIcons = new Dictionary<SkillType, GameObject>();
    public TextMeshProUGUI score;
    public Color countWarningColor;
    //public BarController eraserBar;

    [Header("End game")]
    public GameObject gameOverScreen;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreEndText;


    public void Awake()
    {
        //Singleton
        if (self == null)
            self = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SetMainMenuShow(true);
        SetGameUIShow(false);
        gameOverScreen.SetActive(false);
        UpdateScore(0);

        filledTitle.material.SetFloat(_fillinfShaderParam, inkFillingRange.x);
    }


    #region MAIN MENU

    public void SetMainMenuShow(bool show)
    {
        _mainMenu.SetActive(show);
        //TODO:
    }

    public void SetGameUIShow(bool show)
    {
        _inGameUI.SetActive(show);
        //TODO:
    }

    public void StartGameTransition()
    {
        if(startGameCoroutine == null)
        {
            startGameCoroutine = StartCoroutine(StartTransitionCorroutine());
        }
    }

    //Transicion completa de inicio de la partida
    private IEnumerator StartTransitionCorroutine()
    {
        float currTime = 0;

        //Mostrar fondo poco a poco acelerando su movimiento
        StartCoroutine(FindObjectOfType<BackgroundController>(true).StartGameCoroutine(fillingTime));

        //Rellenar titulo de tinta
        while (currTime < fillingTime)
        {
            float prop = currTime / fillingTime;

            float filling = Mathf.SmoothStep(inkFillingRange.x, inkFillingRange.y, prop);
            filledTitle.material.SetFloat(_fillinfShaderParam, filling);

            yield return new WaitForEndOfFrame();
            currTime += Time.deltaTime;
        }

        //Efecto de imprimir para mostrar pagina
        FindObjectOfType<CameraMotion>(true).StartPrinting();

        //TODO: Quitar sutilmente la UI de menu
        SetMainMenuShow(false);
    }

    public void EndGameTransition()
    {
        //TODO:
        startGameCoroutine = null;
    }

    #endregion


    #region IN_GAME

    public void StartGameUI()
    {
        //TODO: Mostrar UI in game (contadores y demas)
        SetGameUIShow(true);
        UpdateScore(0);
    }

    //Actualizamos interfaz de puntuacion y contadores primarios
    public void UpdatePrimary(InkColorIndex color, int num)
    {
        if (num <= 3)
            primaryCounts[color].text = num.ToString()+"<color=#"+ ColorUtility.ToHtmlStringRGB(countWarningColor)+">!</color>";
        else
            primaryCounts[color].text = num.ToString();
    }

    //Actualizar contador de puntos
    public void UpdateScore(int _score)
    {
        score.text = _score.ToString();
    }

    //Mostrar pantalla de derrota
    public void Lose(InkColorIndex color, int score)
    {
        gameOverScreen.SetActive(true);
        gameOverText.text = "Out of\n" + color;
        gameOverText.color = InkColors[color];
        scoreEndText.text = "Score: " + score;
    }

    #endregion


    #region FUNCIONES GENERICAS

    //Reiniciar escena
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Salir de la aplicacion
    public void ExitApplication()
    {
        Application.Quit();
    }

    //public void InitEraser(float max)
    //{
    //    eraserBar.InitBar(max);
    //}

    //public bool UpdateEraser(float value)
    //{
    //    return eraserBar.UpdateValue(value);
    //}

    #endregion
}
