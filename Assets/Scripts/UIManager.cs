using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static Globals;

public class UIManager : MonoBehaviour
{
    public static UIManager self;

    public TextMeshProUGUI score;
    public TextMeshProUGUI[] primaryCounts = new TextMeshProUGUI[3];
    public GameObject gameOverScreen;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreEndText;
    public BarController eraserBar;
    public Color countWarningColor;

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
        InitEraser(EraserController.self.eraserCooldown);
        gameOverScreen.SetActive(false);
        UpdateScore(0);
    }

    //Actualizamos interfaz de puntuacion y contadores primarios
    public void UpdatePrimary(InkColorIndex color, int num)
    {
        if (num <= 3)
            primaryCounts[(int)color].text = num.ToString()+"<color=#"+ ColorUtility.ToHtmlStringRGB(countWarningColor)+">!</color>";
        else
            primaryCounts[(int)color].text = num.ToString();
    }

    public void UpdateScore(int _score)
    {
        score.text = _score.ToString();
    }

    public void InitEraser(float max)
    {
        eraserBar.InitBar(max);
    }

    public bool UpdateEraser(float value)
    {
        return eraserBar.UpdateValue(value);
    }

    public void Lose(InkColorIndex color, int score)
    {
        gameOverScreen.SetActive(true);
        gameOverText.text = "Out of\n" + color;
        gameOverText.color = InkColors[(int)color];
        scoreEndText.text = "Score: " + score;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
