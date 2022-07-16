using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;

public class PaintShotController : MonoBehaviour
{
    private GameObject drop;
    private Collider explosionArea;
    public Renderer rend;
    public DropController dropController;

    public InkColorIndex colorIndex;
    public Color color;
    public bool autoDrop = true;
    private bool eraser = false;

    void Awake()
    {
        explosionArea = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        drop = transform.GetChild(0).gameObject;
        dropController = drop.GetComponent<DropController>();
    }

    private void Start()
    {
        //Dejamos caer la gota al inicio si es gota autom�tica
        if(autoDrop)
            Drop();
    }

    //Activamos area de cambio de color de monigotes durante un tiempo y destruimos todo despues
    public IEnumerator Explode()
    {
        explosionArea.enabled = true;
        rend.enabled = false;

        yield return new WaitForSeconds(1f);
        explosionArea.enabled = false;

        yield return new WaitForSeconds(2.5f);
        Destroy(gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        //Cambiar color monigotes dentro del area
        if(other.CompareTag(tagMoninker))
        {
            other.gameObject.GetComponent<MoninkerController>().MoninkerColor = colorIndex;
        }     
    }

    //Dejar caer gota de pintura en vertical
    public void Drop()
    {
        drop.SetActive(true);
        //Hacer que caiga m�s rapido para el tipex
        if (eraser)
            dropController.rb.AddForce(Vector3.down * 20, ForceMode.Impulse);
    }

    //Cambiar color del ret�culo y de la gota de pintura;
    public void SetPaintColor(InkColorIndex _colorIndex)
    {
        colorIndex = _colorIndex;
        color = InkColors[colorIndex];

        if (colorIndex == InkColorIndex.NONE)
            eraser = true;

        dropController.SetColor(color);
        color.a = 0.7f;
        rend.material.color = color;
    }
}
