using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowCursor : MonoBehaviour
{
    public Canvas mainCanvas;
    public float distanceFromCamera = 10;


    [ExecuteAlways]
    private void Awake()
    {
        if(!mainCanvas)
            mainCanvas = FindObjectOfType<Canvas>();
    }

    private void Update()
    {
        var screenPoint = Input.mousePosition;
        screenPoint.z = distanceFromCamera;
        transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
    }
}
