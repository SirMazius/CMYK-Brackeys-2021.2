using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System.Threading.Tasks;

public class CameraMotion : SingletonMono<CameraMotion>
{
    public enum PrintStates
    {
        BEGINING,
        IN_GAME,
        ENDING
    }

    private Transform _cam;
    private float _folioHeight;
    private Quaternion _inGameCameraRot;
    [SerializeField]
    private Vector3 _beginingCameraPos;
    [SerializeField]
    private Vector3 _inGameCameraPos;
    [SerializeField]
    private Vector3 _endingCameraPos;

    public float beginEndOffsetPos;

    [Header("PRINT ANIMATIONS")]
    public int printSteps = 5;
    public float moveTimePercentage = 0.5f;
    public PrintStates state = PrintStates.IN_GAME;

    [Header("GIZMOS")]
    public Transform gizmoUp;
    public Transform gizmoDown;


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
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

    public async Task StartPrinting(float time)
    {
        //Ponemos cámara en posición inicial
        _cam.position = _beginingCameraPos;

        //Calculamos la distancia a mover en cada paso de impresion
        float stepDist = Mathf.Abs(_beginingCameraPos.z - _inGameCameraPos.z) / printSteps;
        float stepTime = time/printSteps * moveTimePercentage;
        float speed = stepDist/stepTime;
        float waitTime = time/printSteps * (1 - moveTimePercentage);

        //Para cada "trompicon" de la impresora...
        for (int i = 0; i < printSteps; i++)
        {
            //Movemos poco a poco rapidamente el folio (la camara en realidad)
            for(float acum = 0; acum < stepDist; )
            {
                await Task.Yield();

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
            await Task.Delay(waitTime.ToMillis());
        }
    }

    [Button]
    public void EndPrinting(float time)
    {
        StartCoroutine(EndPrintAnimation(time));
    }

    private IEnumerator EndPrintAnimation(float time)
    {
        _cam.position = _inGameCameraPos;

        float dist = Mathf.Abs(_endingCameraPos.z - _inGameCameraPos.z);
        float speed = dist / time;

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
