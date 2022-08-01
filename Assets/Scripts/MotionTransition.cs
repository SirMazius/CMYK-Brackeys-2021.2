using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static GameGlobals;


public class MotionTransition : MonoBehaviour
{
    public List<Transform> points = new List<Transform>();


    public void GoToStartPoint(float time, CurveType curve = CurveType.Smoothed)
    {
        StartCoroutine(ParamTransitionOverTime((posX) =>
        { transform.position = new Vector3(posX, transform.position.y, transform.position.z); },
        transform.position.x, points[0].position.x, time, curve));
    }

    public void GoToEndPoint(float time, CurveType curve = CurveType.Smoothed)
    {
        StartCoroutine(ParamTransitionOverTime((posX) =>
        { transform.position = new Vector3(posX, transform.position.y, transform.position.z); },
        transform.position.x, points[1].position.x, time, curve));
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
                points.Add(child.transform);

                UnityEditor.EditorUtility.SetDirty(child);
            }
        }
    }
}
