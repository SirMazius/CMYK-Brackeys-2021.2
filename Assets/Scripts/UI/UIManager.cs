using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static GameGlobals;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using System.Threading.Tasks;

public class UIManager : SingletonMono<UIManager>
{
    [Header("Main menu")]
    public GameObject mainMenuUI;
    private Task _startingTransition = null;

    
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


    private void Start()
    {
        SetMainMenuShow(true);
        SetGameUIShow(false);
        gameOverScreen.SetActive(false);
        UpdateScore(0);
    }


    #region MAIN MENU

    //Funcion a asignar por editor que evita multiples llamadas
    public void StartButtonPressed()
    {
        if (_startingTransition == null || _startingTransition.IsCompleted)
            _startingTransition = StartButtonPressedAsync();
    }

    private async Task StartButtonPressedAsync()
    {
        await TransitionsController.self.StartGameTransition();
        GameManager.self.StartGame();
    }

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
    public async Task Lose(InkColorIndex color, int score)
    {
        gameOverText.text = "Out of\n" + color;
        gameOverText.color = InkColors[color];
        scoreEndText.text = "Score: " + score;

        await TransitionsController.self.EndGameTransition();
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
