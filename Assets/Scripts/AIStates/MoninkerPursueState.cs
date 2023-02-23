using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoninkerController;
using static GameGlobals;

public class MoninkerPursueState : MoninkerState
{
    MoninkerController controller;

    private bool reproducing = false;

    public MoninkerPursueState(MoninkerController contr)
    {
        controller = contr;
    }

    //Se dirige a su objetivo
    public void UpdateState()
    {
        if (controller.currTarget != null && controller.currTarget.MoninkerColor != InkColorIndex.BLACK)
        {
            if((Vector3.Distance(controller.transform.position, controller.currTarget.transform.position) < controller.agent.radius*2f) && !reproducing
                && (GameManager.self.currFrameSpawn < GameManager.self.maxFrameSpawn || controller.MoninkerColor == InkColorIndex.BLACK))
            {
                MoninkerController other = controller.currTarget;

                //Negros tiñen al otro de negro
                if (controller.MoninkerColor == InkColorIndex.BLACK)
                    other.MoninkerColor = InkColorIndex.BLACK;
                //Normales instancian un hijo
                else
                    controller.ReproduceWith(other);
            }
            else
            {
                controller.agent.destination = controller.currTarget.transform.position;
                //Cambiamos la velocidad a más rapido para perseguir
                if(controller.MoninkerColor == InkColorIndex.BLACK)
                    controller.agent.speed = GameManager.self.blackSpeed;
                else
                    controller.agent.speed = GameManager.self.pursueSpeed;
            }

        }
        //En cualquier otro caso volvemos a wander
        else
            StartWander();
    }

    public void Impact() { }

    public void StartPursue() { }

    public void StartDragging()
    {
        if (controller.MoninkerColor != InkColorIndex.BLACK)
        {
            controller.currState = controller.draggingState;
            controller.currTarget = null;
            controller.agent.isStopped = true;
            controller.agent.ResetPath();
            controller.coll.enabled = false;
            controller.agent.radius = grabbedCollRadius;
            controller.EndReproduction();
        }
    }

    public void StartWander()
    {
        controller.currTarget = null;
        controller.currState = controller.wanderState;
        controller.agent.ResetPath();
        controller.agent.radius = normalCollRadius;
        if (controller.MoninkerColor != InkColorIndex.BLACK)
        {
            controller.mainSpriteRender.sprite = UIManager.self.MoninkerIdleSprite;
        }
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
