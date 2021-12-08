using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface MoninkerState
{
    void UpdateState();

    void StartWander();
    void StartPursue();
    void StartDragging();

    void OnTriggerEnter(Collider coll);
    void OnTriggerStay(Collider coll);
    void OnTriggerExit(Collider coll);
}

