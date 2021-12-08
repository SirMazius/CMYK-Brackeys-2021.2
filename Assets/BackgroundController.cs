using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private Renderer rend;
    private float runTime;

    public float inGameSpeed = 0.5f;
    public float menuSpeed = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    //Desplazar fondo en funcion mas rapido en menu que en juego
    void Update()
    {
        if (GameManager.self != null && GameManager.self.inGame)
            runTime += Time.deltaTime * inGameSpeed;
        else
            runTime += Time.deltaTime * menuSpeed;

        rend.material.SetTextureOffset("_BaseMap", new Vector2(runTime, 0));
    }
}
