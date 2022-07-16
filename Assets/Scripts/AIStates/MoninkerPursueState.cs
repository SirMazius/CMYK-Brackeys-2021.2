using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoninkerController;
using static GameGlobals;

public class MoninkerPursueState : MoninkerState
{
    MoninkerController controller;

    public MoninkerPursueState(MoninkerController contr)
    {
        controller = contr;
    }

    //Se dirige a su objetivo
    public void UpdateState()
    {
        if (controller.currTarget != null && GameManager.self.currFrameSpawn< GameManager.self.maxFrameSpawn)
        {
            if(Vector3.Distance(controller.transform.position, controller.currTarget.transform.position) < controller.agent.radius * 2f)
            {
                //Instanciar un nuevo monigote y resetear padres
                Reproduce(controller.currTarget.GetComponent<MoninkerController>());
            }
            else
            {
                controller.agent.destination = controller.currTarget.position;
                //Cambiamos la velocidad a más rapido para perseguir
                if(controller.MoninkerColor == InkColorIndex.BLACK)
                    controller.agent.speed = blackSpeed;
                else
                    controller.agent.speed = pursueSpeed;
            }

        }
        //En cualquier otro caso volvemos a wander
        else
            StartWander();
    }

    //Instanciar un nuevo monigote y resetear padres
    public void Reproduce(MoninkerController other)
    {
        //Instanciar hijo combinando colores
        controller.ReproduceWith(other);
        GameManager.self.currFrameSpawn++;

        //Pasar de nuevo a wander y reiniciar celo
        StartWander();
        other.currState.StartWander();
        controller.wanderState.currHeatTime = 0;
        other.wanderState.currHeatTime = 0;
        //Debug.Log("Ha habido FOLLE");
    }

    public void Impact() { }

    public void StartPursue() { }

    public void StartDragging()
    {
        controller.currState = controller.draggingState;
        controller.currTarget = null;
        controller.agent.isStopped = true;
        controller.agent.ResetPath();
        controller.coll.enabled = false;
        controller.agent.radius = grabbedCollRadius;
    }

    public void StartWander()
    {
        controller.currTarget = null;
        controller.currState = controller.wanderState;
        controller.agent.ResetPath();
        controller.agent.radius = normalCollRadius;
    }

    public void OnTriggerEnter(Collider coll)
    {
    }

    //Si esta cerca el jugador o una concha...
    public void OnTriggerStay(Collider coll) { }

    //Si está persiguiendo al jugador y sale de su radio de huida se vuelve a wander
    public void OnTriggerExit(Collider coll)
    {
    }
}
