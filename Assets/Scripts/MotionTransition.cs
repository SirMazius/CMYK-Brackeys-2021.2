using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static GameGlobals;
using DG.Tweening;

public class MotionTransition : SerializedMonoBehaviour
{
    public List<Transform> points = new List<Transform>();


    [Button]
    public void GoToStartPoint(float time = 1, CurveType curve = CurveType.Smoothed)
    {
        StartCoroutine(ParamTransitionOverTime((posX) =>
        { transform.position = new Vector3(posX, transform.position.y, transform.position.z); },
        transform.position.x, points[0].position.x, time, curve));
    }

    [Button]
    public Tween GoToEndPoint(float time = 1, CurveType curve = CurveType.Smoothed)
    {
        return transform.DOMoveX(points[1].position.x, time).SetEase(Ease.InOutCubic);

        //StartCoroutine(ParamTransitionOverTime((posX) =>
        //{ transform.position = new Vector3(posX, transform.position.y, transform.position.z); },
        //transform.position.x, points[1].position.x, time, curve));
    }

    [Button("Update Points")]
    private void OnValidate()
    {
        if (points == null)
            points = new List<Transform>();

        bool empty = false;
        for(int i=0; i< points.Count; i++)
        {
            if (points[i] == null)
            {
                points.RemoveAt(i);
                i--;
                empty = true;
            }
            else if(empty)
            {
                Destroy(points[i]);
                points.RemoveAt(i);
            }
        }

        //Preparamos los puntos de la transicion
        for(int i = 0; i<2; ++i)
        {
            if(points.Count<=i)
            {
                GameObject child = new GameObject("Point " + i);
                child.transform.parent = transform;
                child.gameObject.AddComponent<Gizmo>();
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
                child.transform.parent = transform.parent;
                points.Add(child.transform);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(child);
#endif
            }
        }
    }
}
