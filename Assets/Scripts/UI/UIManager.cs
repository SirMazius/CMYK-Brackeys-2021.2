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
    public TextMeshProUGUI timer;
    public Color countWarningColor;
    public TextMeshProUGUI comboText;
    public List<SkillExchanger> exchangers;

    [Header("End game")]
    public GameObject gameOverScreen;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreEndText;

    [Header("Highscores")]
    public GameObject highscoresScreen;
    public GameObject scoreRecordPrefab;
    public Transform scoreRecordsPanel;
    public Color highlightedScoreColor;

    [Header("Tutorial")]
    public GameObject tutorialScreen;
    public List<GameObject> tutorialPages;
    public List<GameObject> tutorialArrows;
    private int currentTutorial = 0;

    [Header("Back fade")]
    public Image BlackScreen;
    public float BlackFadeDuration = 2f;
    private Tween BlackFadeTween = null;

    [Header("Tweens parameters")]
    public float ComboTweenDuration = 0.8f;
    public float ComboHideTweenDuration = 0.4f;
    public Vector3 ComboTweenScale = new Vector3(-0.2f, 0.4f, 0);
    public float ComboTweenShake = 10f;
    public float BlackWaveDuration = 1.2f;
    public float BlackWaveMaxScale = 0.5f;

    [Header("Moninker Sprites")]
    public Sprite MoninkerIdleSprite;
    public Sprite MoninkerHeatSprite;
    public Sprite EvilMoninkerSprite;
    public Dictionary<InkColorIndex, Color> InkColors = new Dictionary<InkColorIndex, Color>
    {
        {InkColorIndex.CYAN, new Color32(84,236,255,255)}, // Cyan #54ECFF
        {InkColorIndex.MAGENTA, new Color32(255,99,180,255)}, // Magenta #FF63B4
        {InkColorIndex.YELLOW, new Color32(254,233,70,255)}, // Yellow #FEE946 //new Color32(255,236,92,255),// Yellow #FFEC5C 
        {InkColorIndex.RED, new Color32(255,141,92,255)}, // Red #FF8D5C
        {InkColorIndex.GREEN, new Color32(140,255,84,255)}, // Green #8CFF54
        {InkColorIndex.BLUE, new Color32(99,110,255,255)}, // Blue #636EFF
        {InkColorIndex.BLACK, new Color32(48,48,48,255)}, // Black #303030
        {InkColorIndex.NONE, new Color32(180,180,180,255)} // Borrador #C9C9C9
    };


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
        GameManager.self.PrepareNewGame();
        await TransitionsController.self.StartGameTransition();
    }

    public void SetMainMenuShow(bool show)
    {
        mainMenuUI.SetActive(show);
        //TODO:
    }

    public void SetGameUIShow(bool show)
    {
        inGameUI.SetActive(show);
        tutorialScreen.SetActive(!show);
        //TODO:
    }

    public void SetHighscoresShow(bool show)
    {
        highscoresScreen.SetActive(show);
    }

    #endregion


    #region TUTORIAL

    //Mostrar/ocultar tutoriales
    public void SetTutorialShow(bool show)
    {
        if(show)
        {
            currentTutorial = 0;
            ChangeTutorialPage(currentTutorial, false);
        }

        tutorialScreen.SetActive(true);

        var anim = tutorialScreen.GetComponent<Animator>();
        if(anim.GetBool("Show")!=show)
            tutorialScreen.GetComponent<Animator>().SetBool("Show", show);

        AudioManager.self.PlayOverriding(SoundId.InkFill);
    }

    //Mostrar una pagina concreta del tutorial
    private void ChangeTutorialPage(int page, bool playsound)
    {
        for(int i = 0; i< tutorialPages.Count; i++)
        { 
            tutorialPages[i].SetActive(i == page);
        }

        tutorialArrows[0].SetActive(page != 0);
        tutorialArrows[1].SetActive(true);

        if (playsound)
            AudioManager.self.PlayAdditively(SoundId.Printer_Button);
    }

    //Pasar a la siguiente pagina o a la anterior
    public void NextTutorialPage(int pageStep)
    {
        int prevTutorial = currentTutorial;

        currentTutorial += pageStep;
        currentTutorial = Mathf.Max(currentTutorial, 0);

        //Fin de los tutoriales
        if(currentTutorial >= tutorialPages.Count)
            SetTutorialShow(false);
        //Cambiar a otra página
        else if(currentTutorial != prevTutorial)
            ChangeTutorialPage(currentTutorial, true);
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

    //Actualizar contador de puntos
    public void UpdateTimer(float time)
    {
        int totalSeconds = Mathf.FloorToInt(time);
        int mins = totalSeconds / 60;
        int secs = totalSeconds % 60;

        timer.text = string.Format("{0:0}:{1:00}", mins, secs);
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

    public void SetLaunchMode(bool launchMode, Skill skill = null)
    {
        //TODO: Mostrar letrero y efectos de en lanzamiento
        //Actualizar UI mostrando claramente que estamos en modo lanzamiento (sonido de alerta o algo y un efecto muy visible continuo)
        UISkillLauncher.self.Show(launchMode, skill);
        foreach(var exchanger in exchangers)
        {
            exchanger.gameObject.SetActive(!launchMode);
        }

        if(launchMode)
            AudioManager.self.PlayOverriding(SoundId.Select_launch);
    }

    #endregion


    #region ENDGAME

    //Mostrar pantalla de derrota
    public async Task ShowGameOverUI(InkColorIndex color, int score)
    {
        gameOverText.text = "Out of\n" + color;
        gameOverText.color = InkColors[color];
        scoreEndText.text = "Score: " + score;

        await TransitionsController.self.GameOverTransition();
    }

    public async Task ShowHighscores(int currentScore = -1, bool highlightRecord = true)
    {
        SetHighscoresShow(true);

        if (currentScore < 0)
            highlightRecord = false;

        //Destruimos todos los registros previos de puntuaciones en UI
        foreach(Transform child in scoreRecordsPanel)
        {
            Destroy(child.gameObject);
        }
        
        //Creamos los registros con las nuevas puntuaciones
        for (int i = 0; i < HighscoreManager.self.ScoreRecords.Count; i++)
        {
            HighscoreManager.ScoreRecord score = HighscoreManager.self.ScoreRecords[i];
            GameObject recordGO = Instantiate(scoreRecordPrefab, scoreRecordsPanel);
            TextMeshProUGUI recordText = recordGO.GetComponentInChildren<TextMeshProUGUI>(true);
            recordText.text = (i+1) + ".  " + score.Score;

            //Resaltar resultado actual
            if(highlightRecord && score.Score == currentScore)
            {
                recordText.color = highlightedScoreColor;
                highlightRecord = false;
            }
        }

        await TransitionsController.self.FlashHighScoreTransition();
    }

    #endregion


    #region BLACK FADE

    public void DoBlackFade(bool toBlack)
    {
        if(BlackFadeTween != null)
            BlackFadeTween.Kill(false);

        if (toBlack)
            BlackFadeTween = BlackScreen.DOFade(1, BlackFadeDuration).ChangeStartValue(0);
        else
            BlackFadeTween = BlackScreen.DOFade(0, BlackFadeDuration).ChangeStartValue(1);
    }

    #endregion


    #region FUNCIONES GENERICAS

    //Reiniciar escena
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Salir de la aplicacion con un fade
    public void ExitApplication()
    {
        DoBlackFade(true);
        BlackFadeTween.OnComplete(() => Application.Quit());
    }

    #endregion
}
