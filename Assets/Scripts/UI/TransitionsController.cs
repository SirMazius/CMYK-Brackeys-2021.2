using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class TransitionsController : SingletonMono<TransitionsController>
{
    private Coroutine currentTransitionCoroutine = null;
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


    public async Task StartGame()
    {
        await StartGameTask();
    }

    //Transicion completa de inicio de la partida
    private async Task StartGameTask()
    {
        //Rellenar titulo
        titleFiller.StartCompleteFill(titleFillTime);
        //yield return new WaitForSeconds(1);
        await Task.Delay(1000);

        //Mostrar fondo poco a poco acelerando su movimiento
        _background.Show(bgAppearingTime);
        _background.StartMoving(bgSpeedTime);
        //Mover titulo al centro
        titleMotion.GoToEndPoint(bgAppearingTime);

        await Task.Delay((int)((bgAppearingTime - 1) *1000));
        //yield return new WaitForSeconds(bgAppearingTime - 1);

        //Efecto de imprimir para mostrar pagina
        _cameraMotion.StartPrinting(printTime);

        await Task.Delay((int)(printTime*1000));
        //yield return new WaitForSeconds(printTime);

        //TODO: aparicion ui in game
        //SetMainMenuShow(false);
        currentTransitionCoroutine = null;
    }


    public async void EndGame()
    {
        if (currentTransitionCoroutine == null)
        {
            currentTransitionCoroutine = StartCoroutine(EndGameTransitionCoroutine());
        }
    }

    private IEnumerator EndGameTransitionCoroutine()
    {
        //inGameUI.SetActive(false);
        //gameOverScreen.SetActive(true);

        yield return new WaitForSeconds(3);

        //gameOverScreen.SetActive(false);
        _cameraMotion.EndPrinting(endPrintTime);
        _background.StartStopping(bgSpeedTime);
        //SetMainMenuShow(true);
        titleFiller.StartCompleteDrain(bgAppearingTime);
        titleMotion.GoToStartPoint(bgAppearingTime);

        yield return new WaitForSeconds(1);

        _background.Hide(bgAppearingTime);

        yield return new WaitForSeconds(bgAppearingTime);
        currentTransitionCoroutine = null;
    }
}
