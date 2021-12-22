using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class GrabberController : MonoBehaviour
{
    public static GrabberController self;
    public List<MoninkerController> grabbedMoninkers;
    public Vector3 cursor { get => GetCursorFloorPoint();}
    public float radius;
    public Vector3[] offsets;


    void Awake()
    {
        //Singleton
        if (self == null)
            self = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        //Comenzar el grab seleccionando a los monigotes implicados
        if (Input.GetMouseButtonDown(0))
        {
            //Obtenemos los monikers cercanos a donde se ha clicado
            Vector3 point = cursor;
            GameManager.self.GetMoninkersInRadius(point, radius, out grabbedMoninkers);

            //Tomamos el color del moninker mas cercano y cogemos solo los monigotes de ese color
            InkColorIndex color = GetNearestMoninkerInList(point, grabbedMoninkers).color;
            for (int i = 0; i < grabbedMoninkers.Count; i++)
            {
                if (grabbedMoninkers[i].color != color)
                {
                    grabbedMoninkers.RemoveAt(i);
                    i--;
                }
            }

            //Almacenamos los offsets respecto al punto clicado y pasamos a dragging
            offsets = new Vector3[grabbedMoninkers.Count];
            for (int i = 0; i < grabbedMoninkers.Count; i++)
            {
                MoninkerController m = grabbedMoninkers[i];
                offsets[i] = m.transform.position - point;
                m.currState = m.draggingState;
            }
        }
        //Mover monigotes
        else if (Input.GetMouseButton(0) && grabbedMoninkers != null)
        {
            Vector3 point = cursor;
            for (int i = 0; i < grabbedMoninkers.Count; i++)
                grabbedMoninkers[i].transform.position = cursor + offsets[i];
        }
        //Soltar monigotes y pasar a wander
        else if(Input.GetMouseButtonUp(0))
        {
            for (int i = 0; i < grabbedMoninkers.Count; i++)
            {
                MoninkerController m = grabbedMoninkers[i];
                m.currState = m.wanderState;
            }
            grabbedMoninkers.Clear();
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
