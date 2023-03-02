using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static GameGlobals;
using DG.Tweening;


public class GrabberController : SingletonMono<GrabberController>
{
    private bool _grabbing = false;
    [SerializeField]
    private bool _grabAvailable = true;
    [SerializeField]
    private bool _inCombo = false;
    [HideInInspector]
    public UnityEvent<int> OnGrabbedsChange = new UnityEvent<int>();
    
    [Header("Grabbeds")]
    public List<MoninkerController> grabbedMoninkers;
    public InkColorIndex grabbedsColor = InkColorIndex.NONE;
    public float grabRadius = 0.5f;
    public float grabDelay = 0.1f;
    public float maxComboPitchIncrement = 0.5f;
    public int maxComboMoninkers = 40;
    private Coroutine _delayCoroutine = null; 
    private Coroutine _comboTimerCoroutine = null;

    [Header("Atracción moninkers")]
    public float attractRadius = 10;
    public float normalAttractForce = 3f;
    public float sameColorAttractForce = 5f;
    public float comboMaxDelay = 1.5f;
    public float maxGrabbedsPerFrame = 5;

    private Vector3 _lastCursorPos = new Vector3();
    private Vector3 _firstGrabbedPos;


    void Update()
    {
        //Si se hace click sobre el escenario (no UI) se comienza a atraer moninkers
        if (Input.GetMouseButtonDown(0) && !InputModule.OveredUIElement)
            StartGrabMoninkers(GameGlobals.Cursor);
        //Si se esta agarrando o atrayendo moninkers controlamos el drag y el drop
        else if(_grabbing)
        {
            if (Input.GetMouseButton(0))
                WhileGrabbingMoninkers(GameGlobals.Cursor);
            else if (Input.GetMouseButtonUp(0))
                EndGrabMoninkers();
        }

        _lastCursorPos = GameGlobals.Cursor;
    }


    #region GRABBING

    //Comenzar el grab seleccionando a los monigotes implicados
    public void StartGrabMoninkers(Vector3 point)
    {
        grabbedMoninkers.Clear();
        OnGrabbedsChange.Invoke(0);
        _grabbing = true;
        _grabAvailable = true;
        if(GameManager.self.IsInGame)
            AudioManager.self.PlayOverriding(SoundId.Absorbing);
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
                TryGrabIfNear(point, attracteds);
            }
        }
        //Hay moninkers agarrados
        else
        {
            //Si sigue en combo se intentan agarrar más cercanos cortando combo cuando se acerque otro color
            if (_inCombo)
            {
                attracteds = AttractMoninkers(point);
                TryGrabIfNear(point, attracteds);
            }

            //Desplazar agarrados
            for (int i = 0; i < grabbedMoninkers.Count; i++)
                grabbedMoninkers[i].transform.position = point + grabbedMoninkers[i].grabOffset;
                //grabbedMoninkers[i].transform.position += (point - _lastCursorPos);
        }
    }

    //Soltar monigotes y pasar a wander
    public void EndGrabMoninkers()
    {
        _grabbing = false;
        _inCombo = false;

        //Exchanger sobre el que esta el cursor en este momento
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
            //Si se sueltan los moninkers fuera del mapa los dejamos en la posicion del primer grab
            if(CursorHoveredGO != GameManager.self.floor.gameObject)
                m.transform.position = _firstGrabbedPos + m.grabOffset;

            m.currState.StartWander();
        }
        
        grabbedMoninkers.Clear();
        grabbedsColor = InkColorIndex.NONE;
        UIManager.self.UpdateComboCount(0);

        AudioManager.self.Stop(SoundId.Absorbing);

        OnGrabbedsChange.Invoke(0);
    }

    //Coger moninkers en radio de grab del mismo color. Devuelve los moninkers del color adecuado
    private int TryGrabIfNear(Vector3 point, List<MoninkerController> moninkers)
    {
        int sameColorMoninkers = 0;
        int frameGrabbeds = 0;

        if(_grabAvailable)
        {
            foreach(MoninkerController m in moninkers)
            {
                //Esta en el radio de grab, se puede agarrar
                if (!m.grabbed && Vector3.Distance(m.transform.position, point) < grabRadius)
                {
                    //Si es del mismo color o no hay color predefinido
                    if (grabbedsColor == InkColorIndex.NONE || m.MoninkerColor == grabbedsColor)
                    {
                        sameColorMoninkers++;

                        //En este frame todavia no se ha alcanzado el maximo de agarrados
                        if(frameGrabbeds < maxGrabbedsPerFrame)
                        {
                            //Comenzar a agarrar, seleccionando el color que se va a agarrar
                            if(grabbedMoninkers.Count == 0)
                            {
                                grabbedsColor = m.MoninkerColor;
                                _inCombo = true;
                            }

                            GrabMoninker(m, point);

                            frameGrabbeds++;
                        }
                    }
                }

            }

            if(frameGrabbeds>0)
            {
                //Reproducir sonido y Delays de despues de agarrar en este frame
                FinishGrabThisFrame();
            }
        }

        return sameColorMoninkers;
    }

    private void FinishGrabThisFrame()
    {
        //Delay entre grabs
        if (_delayCoroutine != null)
            StopCoroutine(_delayCoroutine);
        _delayCoroutine = StartCoroutine(DelayGrab(grabDelay));

        //Tiempo sin coger moninkers corta el combo
        if (_comboTimerCoroutine != null)
            StopCoroutine(_comboTimerCoroutine);
        _comboTimerCoroutine = StartCoroutine(ComboBreakTimer(comboMaxDelay));

        //Sonido agarre al final del frame con un pitch variable según los grabbeds
        AudioSource source;
        Sound sound = AudioManager.self.PlayInNewSource(SoundId.Combo_grab, out source);
        source.pitch = DOVirtual.EasedValue(sound.originalPitch, sound.originalPitch + maxComboPitchIncrement, Mathf.Clamp01(grabbedMoninkers.Count / (float)maxComboMoninkers), Ease.OutSine);

        //Actualizar UI
        UIManager.self.UpdateComboCount(grabbedMoninkers.Count, grabbedsColor);
    }

    //Pasar a cogido un moninker anotando su offset y cambiando a estado dragging
    private void GrabMoninker(MoninkerController moninker, Vector3 point)
    {
        grabbedMoninkers.Add(moninker);

        moninker.grabOffset = moninker.transform.position - point;
        moninker.currState.StartDragging();
        moninker.grabbed = true;

        //Almacenar posicion inicial del primer moninker cogido en caso de soltar fuera del mapa
        _firstGrabbedPos = moninker.transform.position;

        //Avisar a exchangers del numero actual de cogidos
        OnGrabbedsChange.Invoke(grabbedMoninkers.Count);
    }

    private void ComboBreak()
    {
        if(_inCombo)
        {
            _inCombo = false;
            AudioManager.self.Stop(SoundId.Absorbing);
            UIManager.self.comboText.color = UIManager.self.InkColors[InkColorIndex.NONE];
            AudioManager.self.PlayAdditively(SoundId.Combo_break);
        }
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
            float attraction = (m.MoninkerColor == grabbedsColor) ? sameColorAttractForce : normalAttractForce;
            float forceMag = ParabolicDecrease(distance, attractRadius, attraction);
            
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
            if (currDist < nearestDist && m.MoninkerColor != InkColorIndex.BLACK)
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

    private IEnumerator DelayGrab(float time)
    {
        _grabAvailable = false;
        yield return new WaitForSecondsRealtime(time);
        _grabAvailable = true;
    }


    private IEnumerator ComboBreakTimer(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        ComboBreak();
    }

    #endregion
}
