using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static GameGlobals;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class UIManager : SingletonMono<UIManager>
{
    [Header("Main menu")]
    public GameObject mainMenuUI;
    public ImageFiller titleFiller;
    public MotionTransition titleMotion;
    private CameraMotion _cameraMotion;
    private BackgroundController _background;
    private Coroutine currentTransitionCoroutine = null;
    public float bgAppearingTime = 1.5f;
    public float titleFillTime = 3f;
    public float bgSpeedTime = 3f;
    public float printTime = 2;
    public float endPrintTime = 2;

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
        if(currentTransitionCoroutine == null)
        {
            currentTransitionCoroutine = StartCoroutine(StartGameTransitionCoroutine());
        }
    }

    //Transicion completa de inicio de la partida
    private IEnumerator StartGameTransitionCoroutine()
    {
        //Rellenar titulo
        titleFiller.StartCompleteFill(titleFillTime);
        yield return new WaitForSeconds(1);

        //Mostrar fondo poco a poco acelerando su movimiento
        _background.Show(bgAppearingTime);
        _background.StartMoving(bgSpeedTime);
        //Mover titulo al centro
        titleMotion.GoToEndPoint(bgAppearingTime);

        yield return new WaitForSeconds(bgAppearingTime-1);

        //Efecto de imprimir para mostrar pagina
        _cameraMotion.StartPrinting(printTime);
        
        yield return new WaitForSeconds(printTime);

        //TODO: aparicion ui in game
        SetMainMenuShow(false);
        currentTransitionCoroutine = null;
    }

    public void EndGameTransition()
    {
        if (currentTransitionCoroutine == null)
        {
            currentTransitionCoroutine = StartCoroutine(EndGameTransitionCoroutine());
        }
    }

    private IEnumerator EndGameTransitionCoroutine()
    {
        inGameUI.SetActive(false);
        gameOverScreen.SetActive(true);
        
        yield return new WaitForSeconds(3);

        gameOverScreen.SetActive(false);
        _cameraMotion.EndPrinting(endPrintTime);
        _background.StartStopping(bgSpeedTime);
        SetMainMenuShow(true);
        titleFiller.StartCompleteDrain(bgAppearingTime);
        titleMotion.GoToStartPoint(bgAppearingTime);

        yield return new WaitForSeconds(1);

        _background.Hide(bgAppearingTime);

        yield return new WaitForSeconds(bgAppearingTime);
        currentTransitionCoroutine = null;
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
    [Button]
    public void Lose(InkColorIndex color, int score)
    {
        gameOverText.text = "Out of\n" + color;
        gameOverText.color = InkColors[color];
        scoreEndText.text = "Score: " + score;

        EndGameTransition();
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
