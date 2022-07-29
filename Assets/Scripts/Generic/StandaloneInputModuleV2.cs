using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


//Modificacion del input module para poder obtener el objeto de UI sobre el que esta en todo momento
//https://stackoverflow.com/questions/39150165/how-do-i-find-which-object-is-eventsystem-current-ispointerovergameobject-detect/47412060#47412060
public class StandaloneInputModuleV2 : StandaloneInputModule
{
    private GameObject _overedUIElement = null;
    public GameObject OveredUIElement
    {
        get
        {
            GameObject aux = GetOveredGameObject();
            if (aux != _overedUIElement)
            {
                //Debug.Log("Overed change to: " + aux.name);
                _overedUIElement = aux;
            }
            return _overedUIElement;
        }
    }


    public GameObject GetOveredGameObject(int pointerId = PointerInputModule.kMouseLeftId)
    {
        var lastPointer = GetLastPointerEventData(pointerId);
        if (lastPointer != null)
            return lastPointer.pointerCurrentRaycast.gameObject;
        return null;
    }

    //public GameObject GetOveredGameObject()
    //{
    //    return GetOveredGameObject(PointerInputModule.kMouseLeftId);
    //}
}