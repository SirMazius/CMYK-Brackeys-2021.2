using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class EraserController : MonoBehaviour
{
    public static EraserController self;
    public GameObject eraserPrefab;

    //Borrador
    public bool pressed = false;
    public float eraserCooldown = 10;
    private float currEraserCooldown = 0;
    private Vector3 lastEraserPos;
    public PaintShotController currEraser = null;

    public void Awake()
    {
        //Singleton
        if (self == null)
            self = this;
        else
            Destroy(gameObject);

        //Eraser disponible desde el primer momento
        currEraserCooldown = eraserCooldown;
    }

    public void Update()
    {
        currEraserCooldown += Time.deltaTime;

        //Pulsar para mostrar area de borrado
        if (UIManager.self.UpdateEraser(currEraserCooldown) && !pressed && Input.GetMouseButtonDown(1))
        {
            pressed = true;
            CreateEraser();
        }
        //Soltar clic para disparar borrado
        else if (pressed && !Input.GetMouseButton(1))
        {
            Erase();
            pressed = false;
        }

        //Si se está apuntando con el borrador, cambiamos su posición a lo largo del tablero
        if (currEraser != null)
        {
            lastEraserPos = GetCursorFloorPoint();
            
            if(lastEraserPos != Vector3.positiveInfinity)
                currEraser.transform.position = lastEraserPos;
        }
    }

    //Instanciamos el reticulo de borrador sin llegar a lanzarlo
    public void CreateEraser()
    {
        currEraser = Instantiate(eraserPrefab).GetComponent<PaintShotController>();
        currEraser.SetPaintColor(InkColorIndex.NONE);
        currEraserCooldown = 0;
    }

    //Cuando soltamos clic dejamos caer el tipex y ya no lo movemos de posicion
    public void Erase()
    {
        currEraser.Drop();
        currEraser = null;
    }
}
