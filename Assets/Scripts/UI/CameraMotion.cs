using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CameraMotion : MonoBehaviour
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

        StartCoroutine(StartPrintAnimation());
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

    private IEnumerator StartPrintAnimation()
    {
        //Ponemos cámara en posición inicial
        _cam.position = _beginingCameraPos;

        yield return new WaitForSeconds(1);

        //Calculamos la distancia a mover en cada paso de impresion
        float stepDist = Vector3.Distance(_beginingCameraPos, _inGameCameraPos) / printSteps;
        float stepTime = startPrintTime/printSteps * moveTimePercentage;
        float waitTime = startPrintTime / printSteps * (1 - moveTimePercentage);

        for (int i = 0; i < printSteps; i++)
        {
            for(float acum = 0; acum < stepDist; )
            {
                yield return new WaitForEndOfFrame();

                float incr = stepDist/stepTime * Time.deltaTime; 
                //TODO: ESTA MAL, tiene error
                var vec = _cam.position;
                vec.z += incr;
                _cam.position = vec;

                acum += incr;
            }

            yield return new WaitForSecondsRealtime(waitTime);
        }

        //TODO: evento de empezar partida
    }
}
