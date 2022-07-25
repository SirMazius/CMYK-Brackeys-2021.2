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
    public GameObject mainMenu;

    [Header("In game")]
    public Dictionary<InkColorIndex,TextMeshProUGUI> primaryCounts = new Dictionary<InkColorIndex, TextMeshProUGUI>();
    public Dictionary<SkillType, GameObject> skillsIcons = new Dictionary<SkillType, GameObject>();
    public TextMeshProUGUI score;
    public Color countWarningColor;
    //public BarController eraserBar;

    [Header("End game")]
    public TextMeshProUGUI gameOverText;
    public GameObject gameOverScreen;
    public TextMeshProUGUI scoreEndText;


    public void Awake()
    {
        //Singleton
        if (self == null)
            self = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        //InitEraser(EraserController.self.eraserCooldown);
        gameOverScreen.SetActive(false);
        UpdateScore(0);
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        //TODO:
    }


    //Actualizamos interfaz de puntuacion y contadores primarios
    public void UpdatePrimary(InkColorIndex color, int num)
    {
        if (num <= 3)
            primaryCounts[color].text = num.ToString()+"<color=#"+ ColorUtility.ToHtmlStringRGB(countWarningColor)+">!</color>";
        else
            primaryCounts[color].text = num.ToString();
    }

    public void UpdateScore(int _score)
    {
        score.text = _score.ToString();
    }

    public void Lose(InkColorIndex color, int score)
    {
        gameOverScreen.SetActive(true);
        gameOverText.text = "Out of\n" + color;
        gameOverText.color = InkColors[color];
        scoreEndText.text = "Score: " + score;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

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
}
