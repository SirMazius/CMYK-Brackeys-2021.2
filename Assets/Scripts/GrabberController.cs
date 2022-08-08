using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static GameGlobals;

public class GrabberController : MonoBehaviour
{
    public static GrabberController self;
    
    public bool grabbing = false;

    [Header("Atracción moninkers")]
    public List<MoninkerController> grabbedMoninkers;
    public InkColorIndex grabbedsColor = InkColorIndex.NONE;
    public float grabRadius = 0.5f;
    public float attractRadius = 10;
    public float attractForce = 0.01f;
    private bool inCombo = false;
    //TODO: ¿offset de tiempo entre grabbed moninkers?/¿attraction force disminuye con el tiempo?

    public UnityEvent<int> OnGrabbedsChange = new UnityEvent<int>();



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
        //Si se hace click sobre el escenario (no UI) se comienza a atraer moninkers
        if (Input.GetMouseButtonDown(0) && !InputModule.OveredUIElement)
            StartGrabMoninkers(GameGlobals.Cursor);
        //Si se esta agarrando o atrayendo moninkers controlamos el drag y el drop
        else if(grabbing)
        {
            if (Input.GetMouseButton(0))
                WhileGrabbingMoninkers(GameGlobals.Cursor);
            else if (Input.GetMouseButtonUp(0))
                EndGrabMoninkers();
        }
    }


    #region GRABBING

    //Comenzar el grab seleccionando a los monigotes implicados
    public void StartGrabMoninkers(Vector3 point)
    {
        grabbedMoninkers.Clear();
        OnGrabbedsChange.Invoke(0);
        grabbing = true;
    }

    //Al mantener y arrastrar se atraen moninkers hasta agarrarlos, pudiendo desplazarlos completamente
    public void WhileGrabbingMoninkers(Vector3 point)
    {
        //Desplazar atrayendo moninkers cercanos
        List<MoninkerController> attracteds;

        //No se ha comenzado a coger moninkers
        if(grabbedMoninkers.Count == 0)
        {
            attracteds = AttractMoninkers(point);

            //Obtenemos los monikers en el area cercana a donde se ha clicado
            List<MoninkerController> nearMoninkers;
            GameManager.self.GetMoninkersInRadius(point, grabRadius, out nearMoninkers);

            //Tomamos el color del moninker mas cercano y cogemos solo los monigotes de ese color
            var nearest = GetNearestMoninkerInList(point, nearMoninkers);
            if (nearest && nearest.MoninkerColor != InkColorIndex.NONE)
            {
                grabbedsColor = nearest.MoninkerColor;
                inCombo = !TryGrabIfNear(point, attracteds);
            }
        }
        //Hay moninkers agarrados
        else
        {
            //Si sigue en combo se intentan agarrar más cercanos cortando combo cuando se acerque otro color
            if (inCombo)
            {
                attracteds = AttractMoninkers(point);
                inCombo = !TryGrabIfNear(point, attracteds);
            }

            //Desplazar agarrados
            for (int i = 0; i < grabbedMoninkers.Count; i++)
                grabbedMoninkers[i].transform.position = point + grabbedMoninkers[i].grabOffset;
        }
    }

    //Soltar monigotes y pasar a wander
    public void EndGrabMoninkers()
    {
        grabbing = false;
        inCombo = false;

        //Exchanger sobre el que esta el cursor en este momento
        //TODO: Controlar on hover enter y exit de exchangers
        SkillExchanger OveredExchanger = null;

        //Se puede canjear moninkers por habilidades
        if (IsOverExchanger(out OveredExchanger))
        {
            //Si se cumplen las condiciones de intrercambio, se eliminan los mininkers intercambiados
            if(OveredExchanger.TryExchange(grabbedMoninkers.Count, grabbedsColor))
            {
                DestroySeveralGrabbeds(OveredExchanger.ExchangeQuantity);
            }
        }
        
        //Se sueltan los moninkers que no han sido intercambiados
        for (int i = 0; i < grabbedMoninkers.Count; i++)
        {
            MoninkerController m = grabbedMoninkers[i];
            m.currState.StartWander();
            //TODO: Mejorar como se sueltan(contemplar fuera de mapa y tal)
        }
        grabbedMoninkers.Clear();
        OnGrabbedsChange.Invoke(0);
    }

    //Coger moninkers en radio de grab del mismo color o cortar combo si son de otro
    private bool TryGrabIfNear(Vector3 point, List<MoninkerController> moninkers)
    {
        bool breakCombo = false;

        foreach(MoninkerController m in moninkers)
        {
            if(Vector3.Distance(m.transform.position, point) < grabRadius)
            {
                //Comenzar a coger, seleccionando el color que se va a agarrar
                if(moninkers.Count == 0)
                {
                    grabbedsColor = m.MoninkerColor;
                    inCombo = true;
                }

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
        moninker.grabbed = true;

        //Avisar a exchangers del numero actual de cogidos
        OnGrabbedsChange.Invoke(grabbedMoninkers.Count);
    }

    #endregion


    #region ATRAER MONINKERS

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
            float forceMag = ParabolicDecrease(distance, attractRadius, attractForce); //(1 - Mathf.InverseLerp(0, attractRadius, distance)) * attractForce;
            
            //Evitamos que se pase al otro lado del punto
            //Vector3 moveIncrement = distVec.normalized * Mathf.Clamp(forceMag * Time.deltaTime, 0, distance);
            Vector3 moveIncrement = distVec.normalized * forceMag * Time.deltaTime;
            Debug.DrawRay(m.transform.position, distVec.normalized * forceMag * 0.1f, Color.red);
            m.transform.position += moveIncrement;
        }

        return attractedMoninkers;
    }

    //Devuelve el moninker de la lista mas cercano al punto indicado
    private MoninkerController GetNearestMoninkerInList(Vector3 point, List<MoninkerController> moninkers)
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

    #endregion



    #region METODOS AUXILIARES

    //Comprueba si hay moninkers agarrados sobre un exchanger
    private bool IsOverExchanger(out SkillExchanger exchanger)
    {
        //Hay grabbeds y el cursor esta sobre UI
        if(grabbedMoninkers.Count > 0 && UIElementCursor.CurrentHovered)
        {
            //Comprobacion de estar sobre un exchanger
            //exchanger = InputModule.OveredUIElement.GetComponentInParent<SkillExchanger>();
            //TODO:fix
            exchanger = UIElementCursor.CurrentHovered.GetComponentInParent<SkillExchanger>();
            if(exchanger)
                return true;
        }

        exchanger = null;
        return false;
    }

    //Destruye un numero de moninkers de grabbeds
    public void DestroySeveralGrabbeds(int nDestroyed)
    {
        //LImitado por el numero de grabbeds
        nDestroyed = Mathf.Min(nDestroyed, grabbedMoninkers.Count);

        //Quitar de lista de grabbeds y desactivar n moninkers
        for(int i = 0; i<nDestroyed; i++)
        {
            var moninker = grabbedMoninkers[0];
            grabbedMoninkers.Remove(moninker);
            GameManager.self.DeactivateMoninker(moninker);
        }

        OnGrabbedsChange.Invoke(grabbedMoninkers.Count);
    }

    #endregion
}
