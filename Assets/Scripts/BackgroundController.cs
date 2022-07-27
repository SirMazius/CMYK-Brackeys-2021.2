using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private Renderer rend;
    private float runTime;

    public float normalSpeed = 0.5f;
    public Vector2 speedRange = new Vector2(0, 2f);

    private float _currentSpeed = 0;


    // Start is called before the first frame update
    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    //Desplazar fondo en funcion mas rapido en menu que en juego
    private void Update()
    {
        _currentSpeed = Mathf.Clamp(_currentSpeed, speedRange.x, speedRange.y);

        runTime += Time.deltaTime * _currentSpeed;
        rend.material.SetTextureOffset("_BaseMap", new Vector2(runTime, 0));
    }

    //Aparicion de fondo con aumento de velocidad progresivos
    public IEnumerator StartGameCoroutine(float time)
    {
        float currTime = 0;
        float totalSpeedIncr = normalSpeed - speedRange.x;

        while (currTime < time)
        {
            float prop = currTime / time;
            Color col = rend.material.color;

            //Opacidad progresiva
            col.a = prop;
            rend.material.SetColor("_BaseColor", col);

            //Velocidad progresiva
            _currentSpeed = Mathf.SmoothStep(speedRange.x, normalSpeed, prop);

            yield return new WaitForEndOfFrame();
            currTime += Time.deltaTime;
        }
    }
}
