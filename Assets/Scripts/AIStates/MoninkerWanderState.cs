using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoninkerController;

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
        nextHeatTime = Random.Range(minHeatTime,maxHeatTime);
    }
    
    public void UpdateState()
    {
        //Si estamos cerca del punto de destino actual calculamos un nuevo punto aleatorio (o se ha agotado el tiempo para ese destino)
        if (currTargetTime <= 0 || controller.agent.remainingDistance <= controller.agent.stoppingDistance)
        {
            //Obtenemos una direccion aleatoria entre el angulo maximo escogido respecto al forward
            Vector3 direction =
            Quaternion.AngleAxis(Random.Range(0,360), Vector3.up) * Vector3.forward;
            direction.Normalize();
            //Lejania pseudoaleatoria del siguiente punto
            direction *= Random.Range(wanderTargetMinDist, wanderTargetMaxDist);
            Vector3 point = direction + controller.agent.transform.position;
            point.y = controller.transform.position.y;

            if (!controller.agent.SetDestination(point))
                if (!controller.agent.SetDestination(controller.agent.transform.position - direction))
                    controller.agent.destination = Vector3.zero;

            currTargetTime = wanderMaxReachTime;

            //Cambiamos la velocidad a más lento para caminar
            controller.agent.speed = wanderSpeed;

            //¿Hacemos una parada aleatoria al llegar a este punto?
            if (Random.Range(0f, 1f) > wanderWaitProbability)
            {
                //Parada de tiempo aleatorio
                remainingWait = Random.Range(wanderWaitMinTime, wanderWaitMaxTime);
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
        if(!controller.heat)
        {
            currHeatTime += Time.deltaTime;
            if(currHeatTime > nextHeatTime)
            {
                controller.heat = true;
            }
        }

        //Al entrar en celo buscar moninker más cercano de la lista
        if(controller.heat)
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
                        if(m != controller)
                        {
                            controller.currTarget = m.transform;
                            StartPursue();
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
            controller.heat = false;
            currHeatTime = 0;
            nextHeatTime = Random.Range(minHeatTime, maxHeatTime);
            controller.currState = controller.pursueState;
        }
    }

    //Reset del cooldown de celo
    public void StartWander() 
    {
        currHeatTime = 0;
    }
    public void StartDragging()
    {
        controller.currState = controller.draggingState;
        controller.currTarget = null;
        controller.agent.isStopped = true;
        controller.agent.ResetPath();
    }

    public void OnTriggerEnter(Collider coll) { }

    //Si ha pasado cierto tiempo desde que se reprodujo y hay algun monigote cerca, se dirige hacia el
    public void OnTriggerStay(Collider coll)
    {
        //if(coll.CompareTag(Globals.tagMoninker) 
        //    && (currHeatTime > nextHeatTime || (controller.color == Globals.InkColorIndex.BLACK && currHeatTime > blackHeatTime)))
        //{
        //    controller.currTarget = coll.gameObject.transform;
        //    StartPursue();
        //}
    }

    public void OnTriggerExit(Collider coll) { }
}
