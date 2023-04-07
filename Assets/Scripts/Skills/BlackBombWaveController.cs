using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackBombWaveController : MonoBehaviour
{
    public void OnParticleCollision(GameObject other)
    {
        if(other.gameObject.layer == GameGlobals.layerBlackMoninker)
        {
            GameManager.self.DeactivateMoninker(other.GetComponent<MoninkerController>());
        }
    }
}
