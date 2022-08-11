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
using DG.Tweening;


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
    public TextMeshProUGUI comboText;

    [Header("End game")]
    public GameObject gameOverScreen;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreEndText;

    [Header("Tweens parameters")]
    public float ComboTweenDuration = 0.8f;
    public float ComboHideTweenDuration = 0.4f;
    public Vector3 ComboTweenScale = new Vector3(-0.2f, 0.4f, 0);
    public float ComboTweenShake = 10f;


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
    public async Task LoseUI(InkColorIndex color, int score)
    {
        gameOverText.text = "Out of\n" + color;
        gameOverText.color = InkColors[color];
        scoreEndText.text = "Score: " + score;

        await TransitionsController.self.EndGameTransition();
    }

    //Modifica el contador de moninkers cogidos
    public void UpdateComboCount(int count, InkColorIndex color = InkColorIndex.NONE)
    {
        //"Vibrar" al aumentar combo 
        DOTween.Rewind(comboText.transform);
        Tween _comboTextTweenScale = comboText.transform.DOPunchScale(ComboTweenScale, ComboTweenDuration);
        Tween _comboTextTweenPos = comboText.transform.DOShakePosition(ComboTweenDuration, ComboTweenShake);

        if (count > 0)
        {
            comboText.text = count.ToString();
            comboText.color = InkColors[color];
        }
        else
            HideComboCount();
    }

    public void HideComboCount()
    {
        DOTween.Rewind(comboText.transform);
        Tween _comboTextTweenHide = comboText.transform.DOScale(Vector3.zero, ComboHideTweenDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
            comboText.text = "";
            DOTween.Rewind(comboText.transform);
        });
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
