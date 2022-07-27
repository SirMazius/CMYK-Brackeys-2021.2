using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class CameraMotion : MonoBehaviour
{
    public enum PrintStates
    {
        BEGINING,
        IN_GAME,
        ENDING
    }

    public static UnityEvent OnPrintFinished = new UnityEvent();

    private Transform _cam;
    private float _folioHeight;
    private Quaternion _inGameCameraRot;
    [SerializeField]
    private Vector3 _beginingCameraPos;
    [SerializeField]
    private Vector3 _inGameCameraPos;
    [SerializeField]
    private Vector3 _endingCameraPos;

    //public MeshRenderer folioMesh;

    public float beginEndOffsetPos;

    [Header("PRINT ANIMATIONS")]
    public int printSteps = 5;
    public float startPrintTime = 2;
    public float endPrintTime = 1;
    public float moveTimePercentage = 0.5f;
    public PrintStates state = PrintStates.IN_GAME;

    [Header("GIZMOS")]
    public Transform gizmoUp;
    public Transform gizmoDown;



    // Start is called before the first frame update
    private void Awake()
    {
        _cam = Camera.main.transform;
        _folioHeight = Vector3.Distance(gizmoUp.position, gizmoDown.position);
    }

    [Button]
    private void SetOriginalPosition()
    {
        _cam = Camera.main.transform;
        _inGameCameraPos = _cam.position;
        _inGameCameraRot = _cam.rotation;

        Vector3 pos = _inGameCameraPos;
        pos.z = _inGameCameraPos.z - _folioHeight - beginEndOffsetPos;
        _beginingCameraPos = pos;

        pos.z = _inGameCameraPos.z + _folioHeight + beginEndOffsetPos;
        _endingCameraPos = pos;
    }

    private void OnValidate()
    {
        _folioHeight = Vector3.Distance(gizmoUp.position, gizmoDown.position);

        switch (state)
        {
            case PrintStates.BEGINING:
                Camera.main.transform.position = _beginingCameraPos;
                break;
            case PrintStates.IN_GAME:
                Camera.main.transform.position = _inGameCameraPos;
                break;
            case PrintStates.ENDING:
                Camera.main.transform.position = _endingCameraPos;
                break;
        }
    }

    [Button]
    public void StartPrinting()
    {
        StartCoroutine(StartPrintAnimation());
    }

    private IEnumerator StartPrintAnimation()
    {
        //Ponemos cámara en posición inicial
        _cam.position = _beginingCameraPos;

        yield return new WaitForSeconds(1);

        //Calculamos la distancia a mover en cada paso de impresion
        float stepDist = Mathf.Abs(_beginingCameraPos.z - _inGameCameraPos.z) / printSteps;
        float stepTime = startPrintTime/printSteps * moveTimePercentage;
        float speed = stepDist / stepTime;
        float waitTime = startPrintTime / printSteps * (1 - moveTimePercentage);

        //Para cada "trompicon" de la impresora...
        for (int i = 0; i < printSteps; i++)
        {
            //Movemos poco a poco rapidamente el folio (la camara en realidad)
            for(float acum = 0; acum < stepDist; )
            {
                yield return new WaitForEndOfFrame();

                float incr = speed * Time.deltaTime;
                acum += incr;

                //Evitar exceso de desplazamiento (por dt variable)
                if (acum > stepDist)
                    incr -= acum - stepDist;

                var vec = _cam.position;
                vec.z += incr;
                _cam.position = vec;
            }

            //Y hacemos una pausa
            yield return new WaitForSecondsRealtime(waitTime);
        }

        //TODO: evento de empezar partida
        OnPrintFinished.Invoke();
    }

    [Button]
    public void EndPrinting()
    {
        StartCoroutine(EndPrintAnimation());
    }

    private IEnumerator EndPrintAnimation()
    {
        _cam.position = _inGameCameraPos;

        float dist = Mathf.Abs(_endingCameraPos.z - _inGameCameraPos.z);
        float speed = dist / endPrintTime;

        //Movemos poco a poco el folio hasta salid de la pantalla (la camara en realidad)
        for (float acum = 0; acum < dist;)
        {
            yield return new WaitForEndOfFrame();

            float incr = speed * Time.deltaTime;
            acum += incr;

            var vec = _cam.position;
            vec.z += incr;
            _cam.position = vec;
        }
    }
}
