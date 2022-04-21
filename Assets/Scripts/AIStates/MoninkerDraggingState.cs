using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        controller.agent.isStopped = false;
    }

    public void UpdateState()
    {
        //Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 6))
        //{
        //    lastPos = hit.point + controller.dragOffset;
        //    lastPos.y = controller.transform.position.y;

        //    controller.transform.position = lastPos;
        //}
    }
}
