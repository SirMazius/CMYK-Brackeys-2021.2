using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class GrabberController : MonoBehaviour
{
    public float radius;

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            //Obtenemos los monikers cercanos a donde se ha clicado
            Vector3 point = GetCursorFloorPoint();
            List<MoninkerController> moninkers;
            GameManager.self.GetMoninkersInRadius(point, radius, out moninkers);
            InkColorIndex color = GetNearestMoninkerInList(point, moninkers).color;


        }
    }

    public MoninkerController GetNearestMoninkerInList(Vector3 point, List<MoninkerController> moninkers)
    {
        MoninkerController nearest = null;
        float nearestDist = Mathf.Infinity;

        for (int i = 0; i < moninkers.Count; i++)
        {
            MoninkerController m = moninkers[i];
            float currDist = Vector3.Distance(m.transform.position, point);
            if (currDist < nearestDist)
            {
                nearestDist = currDist;
                nearest = m;
            }
        }

        return nearest;
    }
}
