using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class TriggerDetector : MonoBehaviour
{
    public Collider coll;
    public MoninkerController controller;

    // Start is called before the first frame update
    void Awake()
    {
        coll = GetComponent<Collider>();
        controller = GetComponentInParent<MoninkerController>();
        Physics.IgnoreCollision(GetComponentInParent<Collider>(), coll);
    }

    //Detectamos si esta cerca de una pareja válida
    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.CompareTag(tagMoninker) && !controller.dragging)
    //    {
    //        MoninkerController otherContr = other.gameObject.GetComponent<MoninkerController>();
    //        //Nunca se busca a un monigote negro ni que se esta arrastrando
    //        if (otherContr.color != InkColorIndex.BLACK && !otherContr.dragging)
    //        {
    //            //Si es negro solo busca a otros colores para contaminar
    //            //Si no es negro busca a una pareja en celo 
    //            if (controller.color == InkColorIndex.BLACK ||
    //                (controller.color != InkColorIndex.BLACK && otherContr.currState == otherContr.wanderState && otherContr.wanderState.currHeatTime > otherContr.wanderState.nextHeatTime))
    //            {
    //                controller.currState.OnTriggerStay(other);
    //            }
    //        }
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        //TODO: Desactivar trigger cuando se está dragging
        if (other.CompareTag(tagMoninker))
        {
            MoninkerController otherContr = other.gameObject.GetComponent<MoninkerController>();
            //Nunca se busca a un monigote negro ni que se esta arrastrando
            if (otherContr.MoninkerColor != InkColorIndex.BLACK)
            {
                controller.nearMoninkers.Add(otherContr);
            }
        }

        //TODO: POSIBLE SOLUCION: Dividir el terreno en cuadrados, listar a todos los monigotes en la lista del cuadrado en el que estan,y comprobar distancias solo con los monigotes del cuadrado actual y circundantes para encontrar el más cercano.
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagMoninker))
        {
            for (int i = 0; i<controller.nearMoninkers.Count; i++)
            {
                if(controller.nearMoninkers[i].gameObject == other.gameObject)
                {
                    controller.nearMoninkers.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
