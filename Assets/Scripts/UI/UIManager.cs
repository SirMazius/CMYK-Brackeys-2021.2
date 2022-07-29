using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static GameGlobals;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UIManager : SingletonMono<UIManager>
{
    [Header("Main menu")]
    public GameObject mainMenuUI;
    public ImageFiller titleFiller;
    private CameraMotion _cameraMotion;
    private BackgroundController _background;
    private Coroutine startGameCoroutine = null;
    public float bgAppearingTime = 1.5f;
    public float bgSpeedTime = 3f;
    public float printTime = 2;

    [Header("In game")]
    public GameObject inGameUI;
    public Dictionary<InkColorIndex,TextMeshProUGUI> primaryCounts = new Dictionary<InkColorIndex, TextMeshProUGUI>();
    public Dictionary<SkillType, GameObject> skillsIcons = new Dictionary<SkillType, GameObject>();
    public TextMeshProUGUI score;
    public Color countWarningColor;

    [Header("End game")]
    public GameObject gameOverScreen;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreEndText;


    protected override void Awake()
    {
        base.Awake();
        _cameraMotion = CameraMotion.self;
        _background = BackgroundController.self;
    }

    private void Start()
    {
        SetMainMenuShow(true);
        SetGameUIShow(false);
        gameOverScreen.SetActive(false);
        UpdateScore(0);
    }


    #region MAIN MENU

    public void SetMainMenuShow(bool show)
    {
        mainMenuUI.SetActive(show);
        //TODO:
    }

    public void SetGameUIShow(bool show)
    {
        inGameUI.SetActive(show);
        //TODO:
    }

    public void StartGameTransition()
    {
        if(startGameCoroutine == null)
        {
            startGameCoroutine = StartCoroutine(StartTransitionsCorroutine());
        }
    }

    //Transicion completa de inicio de la partida
    private IEnumerator StartTransitionsCorroutine()
    {
        //Rellenar titulo
        titleFiller.StartCompleteFill(bgAppearingTime);
        yield return new WaitForSeconds(1);

        //Mostrar fondo poco a poco acelerando su movimiento
        _background.Show(bgAppearingTime);
        _background.StartMoving(bgSpeedTime);
        //TODO: Quitar sutilmente la UI de menu

        yield return new WaitForSeconds(bgAppearingTime);

        //Efecto de imprimir para mostrar pagina
        _cameraMotion.StartPrinting(printTime);
        //TODO: util aparicion ui in game

        //SetMainMenuShow(false);
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

    #endregion
}
