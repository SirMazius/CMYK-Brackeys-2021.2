using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoninkerController;
using static GameGlobals;

public class MoninkerWanderState : MoninkerState
{
    MoninkerController controller;
    private float currTargetTime = 0;
    private float remainingWait = 0; 
    private float currWanderTime = 0;
    public float currHeatTime = 0;
    public float nextHeatTime;

    public MoninkerWanderState(MoninkerController contr)
    {
        controller = contr;
        //El tiempo hasta el siguiente celo

        nextHeatTime = Random.Range(GameManager.self.minHeatTime, GameManager.self.maxHeatTime);
    }
    
    public void UpdateState()
    {
        //Hasta que no empieza la partida no se mueven
        if (!GameManager.self.IsInGame)
        {
            controller.agent.isStopped = true;
            return;
        }

        //Si estamos cerca del punto de destino actual calculamos un nuevo punto aleatorio (o se ha agotado el tiempo para ese destino)
        if (currTargetTime <= 0 || controller.agent.remainingDistance <= controller.agent.stoppingDistance)
        {
            //Obtenemos una direccion aleatoria entre el angulo maximo escogido respecto al forward
            Vector3 direction =
            Quaternion.AngleAxis(Random.Range(0,360), Vector3.up) * Vector3.forward;
            direction.Normalize();
            //Lejania pseudoaleatoria del siguiente punto
            direction *= Random.Range(GameManager.self.wanderTargetMinDist, GameManager.self.wanderTargetMaxDist);
            Vector3 point = direction + controller.agent.transform.position;
            point.y = controller.transform.position.y;

            if (!controller.agent.SetDestination(point))
                if (!controller.agent.SetDestination(controller.agent.transform.position - direction))
                    controller.agent.destination = Vector3.zero;

            currTargetTime = GameManager.self.wanderMaxReachTime;

            //Cambiamos la velocidad a más lento para caminar
            controller.agent.speed = GameManager.self.wanderSpeed;

            //¿Hacemos una parada aleatoria al llegar a este punto?
            if (Random.Range(0f, 1f) > GameManager.self.wanderWaitProbability)
            {
                //Parada de tiempo aleatorio
                remainingWait = Random.Range(GameManager.self.wanderWaitMinTime, GameManager.self.wanderWaitMaxTime);
            }
        }

        //Mientras dure la pausa paramos al agente
        if (remainingWait > 0)
        {
            controller.agent.isStopped = true;
            remainingWait -= Time.deltaTime;
        }
        else
        {
            controller.agent.isStopped = false;
            currTargetTime -= Time.deltaTime;
        }

        //Actualizamos contador tiempo cooldown de celo
        if(!controller.Heat)
        {
            currHeatTime += Time.deltaTime;

            //El tiempo de celo es mucho menor cuando es negro
            if(currHeatTime > ((controller.MoninkerColor == InkColorIndex.BLACK)? GameManager.self.blackHeatTime: nextHeatTime))
            {
                controller.Heat = true;
            }
        }
        //Al entrar en celo buscar moninker más cercano de la lista
        else
        {
            //MoninkerController nearest = null;
            //float minDist = 100000;
            //foreach(var m in controller.nearMoninkers)
            //{
            //    if (m.color != Globals.InkColorIndex.BLACK && !m.dragging && m.heat)
            //    {
            //        float dist = Vector3.Distance(m.transform.position, controller.transform.position);
            //        if (dist < minDist)
            //        {
            //            minDist = dist;
            //            nearest = m;
            //        }
            //    }
            //}

            //if(nearest != null)
            //{
            //    controller.currTarget = nearest.transform;
            //    StartPursue();
            //}

            Vector2Int[] checkCells = {controller.currCell,
                new Vector2Int(controller.currCell.x-1, controller.currCell.y),
                new Vector2Int(controller.currCell.x+1, controller.currCell.y),
                new Vector2Int(controller.currCell.x, controller.currCell.y+1),
                new Vector2Int(controller.currCell.x, controller.currCell.y-1)
            };
            
            foreach(Vector2Int c in checkCells)
            {
                if(c.x>=0 && c.y>=0 && c.x<GameManager.self.gridX && c.y < GameManager.self.gridZ && GameManager.self.gridLists[c.x,c.y].Count > 0)
                {
                    foreach(MoninkerController m in GameManager.self.gridLists[c.x, c.y])
                    {
                        if(m != controller && !m.reproducing && m.currState is MoninkerWanderState && m.MoninkerColor != InkColorIndex.BLACK && (controller.MoninkerColor == InkColorIndex.BLACK || m.Heat))
                        {
                            controller.currTarget = m;
                            StartPursue();
                            if (controller.MoninkerColor != InkColorIndex.BLACK)
                            {
                                m.currTarget = controller;
                                m.currState.StartPursue();
                            }
                            return;
                        }
                    }
                }
            }
        }
    }

    //Al pasar a pursue aumentamos la velocidad y nos cercioramos que el agente sigue un objetivo
    public void StartPursue()
    {
        if(controller.currTarget != null)
        {
            controller.agent.isStopped = false;
            controller.Heat = false;
            currHeatTime = 0;
            nextHeatTime = Random.Range(GameManager.self.minHeatTime, GameManager.self.maxHeatTime);
            controller.currState = controller.pursueState;
        }
    }

    //Reset del cooldown de celo
    public void StartWander() 
    {
        currHeatTime = 0;
        controller.agent.radius = normalCollRadius;
    }

    public void StartDragging()
    {
        if(controller.MoninkerColor!=InkColorIndex.BLACK)
        {
            controller.currState = controller.draggingState;
            controller.currTarget = null;
            controller.agent.isStopped = true;
            controller.agent.ResetPath();
            controller.coll.enabled = false;
            controller.agent.radius = grabbedCollRadius;
            controller.mainSpriteRender.sprite = UIManager.self.MoninkerIdleSprite;
        }
    }

    public void OnTriggerEnter(Collider coll) { }

    //Si ha pasado cierto tiempo desde que se reprodujo y hay algun monigote cerca, se dirige hacia el
    public void OnTriggerStay(Collider coll) { }

    public void OnTriggerExit(Collider coll) { }
}
