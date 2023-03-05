using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoninkerController;

public class MoninkerDraggingState : MoninkerState
{
    MoninkerController controller;
    Vector3 lastPos = new Vector3(0,0,-1000);

    public MoninkerDraggingState(MoninkerController contr)
    {
        controller = contr;
    }

    public void OnTriggerEnter(Collider coll)
    {
    }

    public void OnTriggerExit(Collider coll)
    {
    }

    public void OnTriggerStay(Collider coll)
    {
    }

    public void StartDragging()
    {
    }

    public void StartPursue()
    {
    }

    //Reanudar agente navmesh al soltar
    public void StartWander()
    {
        controller.currState = controller.wanderState;
        controller.coll.enabled = true;
        controller.agent.isStopped = false;
        controller.agent.radius = normalCollRadius;
        controller.grabbed = false;
        //Reseteamos HeatTime
        controller.wanderState.InterruptHeat();
    }

    public void UpdateState()
    {
    }
}
