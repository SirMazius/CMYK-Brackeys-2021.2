using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class GrabberController : MonoBehaviour
{
    public static GrabberController self;
    public List<MoninkerController> grabbedMoninkers;
    public Vector3 cursor { get => GetCursorFloorPoint();}
    public float grabRadius = 10;
    public Vector3[] offsets;

    [Header("Combinar moninkers")]
    public int minMoninkersCombined = 10;
    public float combineRadius = 10;


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
            StartGrabMoninkers(cursor, grabRadius);
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
            EndGrabMoninkers();
        }
    }

    public void StartGrabMoninkers(Vector3 point, float radius)
    {
        //Obtenemos los monikers cercanos a donde se ha clicado
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

    public void EndGrabMoninkers()
    {
        for (int i = 0; i < grabbedMoninkers.Count; i++)
        {
            MoninkerController m = grabbedMoninkers[i];
            m.currState = m.wanderState;
        }
        grabbedMoninkers.Clear();
    }

    //Combinamos los grabbed moninkers en una habilidad(si hay la suficiente cantidad)
    public void CombineMoninkers()
    {
        if(grabbedMoninkers.Count >= minMoninkersCombined)
        {
            InkColorIndex color = grabbedMoninkers[0].color;

            for(int i=0; i<minMoninkersCombined; i++)
                GameManager.self.DeactivateMoninker(grabbedMoninkers[i]);

            //Instanciamos una nueva habilidad
            SkillsController.self.CreateDyeSkillDrop(color, cursor);
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
