using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using static GameGlobals;
using DG.Tweening;

public class TransitionsController : SingletonMono<TransitionsController>
{
    private Task currentTask = null;

    [Header ("Transitionables")]
    public ImageFiller titleFiller;
    public MotionTransition titleMotion;
    private CameraMotion _cameraMotion;
    private BackgroundController _background;

    [Header("Transition Times")]
    public float bgAppearingTime = 1.5f;
    public float titleFillTime = 3f;
    public float bgSpeedTime = 3f;
    public float printTime = 2;
    public float endPrintTime = 2;


    protected override void Awake()
    {
        base.Awake();

        _cameraMotion = CameraMotion.self;
        _background = BackgroundController.self;
    }

    //Transicion completa de inicio de la partida
    public async Task StartGameTransition()
    {
        List<Task> transitions = new List<Task>();

        //Rellenar titulo
        transitions.Add(titleFiller.StartCompleteFill(titleFillTime).AsyncWaitForCompletion());
        await Task.Delay(1f.ToMillis());

        //Mostrar fondo poco a poco acelerando su movimiento
        transitions.Add(_background.Show(bgAppearingTime).AsyncWaitForCompletion());
        _ =(_background.StartMoving(bgSpeedTime).AsyncWaitForCompletion());
        //Mover titulo al centro
        transitions.Add(titleMotion.GoToEndPoint(bgAppearingTime).AsyncWaitForCompletion());
        await Task.WhenAll(transitions);

        //Efecto de imprimir para mostrar pagina
        await _cameraMotion.StartPrinting(printTime);

        UIManager.self.SetMainMenuShow(false);
        GameManager.self.StartGame();
    }

    public async Task EndGameTransition()
    {
        UIManager.self.inGameUI.SetActive(false);
        UIManager.self.gameOverScreen.SetActive(true);

        await (Task.Delay(3f.ToMillis()));

        UIManager.self.gameOverScreen.SetActive(false);
        UIManager.self.SetMainMenuShow(true);
        _cameraMotion.EndPrinting(endPrintTime);
        _background.StartStopping(bgSpeedTime);
        titleFiller.StartCompleteDrain(bgAppearingTime);
        titleMotion.GoToStartPoint(bgAppearingTime);

        await (Task.Delay(1f.ToMillis()));

        _background.Hide(bgAppearingTime);

        await (Task.Delay(bgAppearingTime.ToMillis()));
    }
}
