using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;

public class GrabberController : MonoBehaviour
{
    public static GrabberController self;
    public Vector3 cursor { get => GetCursorFloorPoint();}

    [Header("Atracción moninkers")]
    public List<MoninkerController> grabbedMoninkers;
    public InkColorIndex grabbedsColor = InkColorIndex.NONE;
    public float grabRadius = 0.5f;
    public float attractRadius = 10;
    public float attractForce = 0.01f;
    private bool inCombo = false;
    //TODO: ¿offset de tiempo entre grabbed moninkers?/¿attractiopn force disminuye con el tiempo?

    //Exchanger sobre el que esta el cursor en este momento
    //TODO: Controlar on hover enter y exit de exchangers
    public SkillExchanger OveredExchanger = null;


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
        if (Input.GetMouseButtonDown(0))
            StartGrabMoninkers(cursor, grabRadius);
        else if (Input.GetMouseButton(0))
            WhileHoldingDown(cursor);
        else if(Input.GetMouseButtonUp(0))
            EndGrabMoninkers();
    }

    
    #region LOGICA DE CLICK

    //Comenzar el grab seleccionando a los monigotes implicados
    public void StartGrabMoninkers(Vector3 point, float radius)
    {
        grabbedMoninkers.Clear();

        //Obtenemos los monikers cercanos a donde se ha clicado
        List<MoninkerController> nearMoninkers;
        GameManager.self.GetMoninkersInRadius(point, radius, out nearMoninkers);

        //Tomamos el color del moninker mas cercano y cogemos solo los monigotes de ese color
        var nearest = GetNearestMoninkerInList(point, nearMoninkers);
        if (nearest && nearest.MoninkerColor != InkColorIndex.NONE)
        {
            grabbedsColor = nearest.MoninkerColor;
            inCombo = true;
        }
    }

    //Al mantener y arrastrar se atraen moninkers hasta agarrarlos, pudiendo desplazarlos completamente
    public void WhileHoldingDown(Vector3 point)
    {
        if (inCombo)
        {
            //Desplazar atrayendo moninkers cercanos
            var attracteds = AttractMoninkers(point);
            //Coger atraidos muy cercanos o cortar combo
            inCombo = !TryGrabIfNear(point, attracteds);
        }

        //Desplazar cogidos
        for (int i = 0; i < grabbedMoninkers.Count; i++)
            grabbedMoninkers[i].transform.position = point + grabbedMoninkers[i].grabOffset;
    }

    //Soltar monigotes y pasar a wander
    public void EndGrabMoninkers()
    {
        inCombo = false;

        //Se puede canjear moninkers por habilidades
        if (IsOverExchanger())
        {
            if(OveredExchanger.TryExchange(grabbedMoninkers.Count, grabbedsColor))
            {
                //TODO: Destruir los X moninkers pedidos
            }
        }
        
        //Se sueltan los moninkers que no han sido intercambiados
        for (int i = 0; i < grabbedMoninkers.Count; i++)
        {
            MoninkerController m = grabbedMoninkers[i];
            m.currState = m.wanderState;
        }
        grabbedMoninkers.Clear();
    }

    #endregion


    //Mover los moninkers cercanos con una fuerza inversamente proporcional a la distancia
    public List<MoninkerController> AttractMoninkers(Vector3 point)
    {
        List<MoninkerController> attractedMoninkers;
        GameManager.self.GetMoninkersInRadius(point, attractRadius, out attractedMoninkers);

        foreach(var m in attractedMoninkers)
        {
            Vector3 distVec = point - m.transform.position;
            distVec = Vector3.ProjectOnPlane(distVec, Vector3.up);
            float distance = distVec.magnitude;

            //Aplicamos un desplazamiento continuo proporcionalmente inverso a la distancia
            float forceMag = CenteredInverseParabola(distance, attractRadius, attractForce); //(1 - Mathf.InverseLerp(0, attractRadius, distance)) * attractForce;
            
            //Evitamos que se pase al otro lado del punto
            Vector3 moveIncrement = distVec.normalized * Mathf.Clamp(forceMag * Time.deltaTime, 0, distance);
            Debug.DrawRay(m.transform.position, distVec.normalized * forceMag * 0.1f, Color.red);
            m.transform.position += moveIncrement;
        }

        return attractedMoninkers;
    }

    //Coger moninkers en radio de grab del mismo color o cortar combo si son de otro
    public bool TryGrabIfNear(Vector3 point, List<MoninkerController> moninkers)
    {
        bool breakCombo = false;

        foreach(MoninkerController m in moninkers)
        {
            if(Vector3.Distance(m.transform.position, point) < grabRadius)
            {
                //Añadir a cogidos si es del mismo color
                if (m.MoninkerColor == grabbedsColor)
                    GrabMoninker(m, point);
                //Si es de otro color, corta el combo para la siguiente iteracion de update
                else
                    breakCombo = true;
            }
        }
        return breakCombo;
    }

    //Pasar a cogido un moninker anotando su offset y cambiando a estado dragging
    private void GrabMoninker(MoninkerController moninker, Vector3 point)
    {
        grabbedMoninkers.Add(moninker);
        moninker.grabOffset = moninker.transform.position - point;
        moninker.currState.StartDragging();
    }

    //Devuelve el moninker de la lista mas cercano al punto indicado
    public MoninkerController GetNearestMoninkerInList(Vector3 point, List<MoninkerController> moninkers)
    {
        MoninkerController nearest = null;
        float nearestDist = Mathf.Infinity;

        for (int i = 0; i < moninkers.Count; i++)
        {
            MoninkerController m = moninkers[i];
            float currDist = Vector3.Distance(m.transform.position, point);
            //TODO: ¿Evitar coger negros?
            if (currDist < nearestDist)
            {
                nearestDist = currDist;
                nearest = m;
            }
        }

        return nearest;
    }


    //Comprueba si hay moninkers agarrados sobre un exchanger
    public bool IsOverExchanger()
    {
        if(grabbedMoninkers.Count > 0 && OveredExchanger!= null)
        {
            //TODO: Comprobacion de estar sobre un exchanger
        }

        return false;
    }
}
