using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;
using DG.Tweening;


public class PaintShotController : MonoBehaviour
{
    private GameObject drop;
    private Collider explosionArea;
    public Renderer rend;
    public DropController dropController;

    public InkColorIndex colorIndex;
    public Color color;

    private AudioSource _fallSound;


    void Awake()
    {
        explosionArea = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        drop = transform.GetChild(0).gameObject;
        dropController = drop.GetComponent<DropController>();
    }

    private void Start()
    {
        //Dejamos caer la gota al inicio si es gota automática
        //if(autoDrop)
        //    Drop();
    }

    //Activamos area de cambio de color de monigotes durante un tiempo y destruimos todo despues
    public IEnumerator Explode()
    {
        _fallSound.Stop();
        Destroy(_fallSound);

        explosionArea.enabled = true;
        rend.enabled = false;
        AudioManager.self.PlayAdditively(SoundId.Paint_explosion);

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
    public void Drop(bool fast)
    {
        float soundDuration = 4;

        drop.SetActive(true);
        Sound sound = AudioManager.self.PlayInNewSource(SoundId.Whistle, out _fallSound);

        //Hacer que caiga más rapido cuando se indique
        if (fast)
        {
            dropController.rb.AddForce(Vector3.down * 20, ForceMode.Impulse);
            soundDuration = 2;
        }

        //Bajar pitch progresivamente conforme cae
        _fallSound.DOPitch(sound.originalPitch - 0.8f, soundDuration).ChangeStartValue(sound.originalPitch).SetEase(Ease.OutCirc);
    }

    //Cambiar color del retículo y de la gota de pintura;
    public void SetPaintColor(InkColorIndex _colorIndex)
    {
        colorIndex = _colorIndex;
        color = InkColors[colorIndex];

        dropController.SetColor(color);
        color.a = 0.7f;
        rend.material.color = color;
    }
}
