using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropController : MonoBehaviour
{
    private Animator animator;
    public Rigidbody rb;
    private MeshRenderer rend;
    private PaintShotController reticle;
    public ParticleSystem tracer, explosion;
    public ParticleSystemRenderer tracerRend, explosionRend;

    public bool exploded = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<MeshRenderer>();
        reticle = GetComponentInParent<PaintShotController>();
        gameObject.SetActive(false);

        //Plano de colision de las particulas
        explosion.collision.SetPlane(0, GameManager.self.floor);
        tracer.collision.SetPlane(0, GameManager.self.floor);
    }

    //Cambiamos el color de la gota principal y las particulas
    public void SetColor(Color color)
    {
        rend.material.color = color;

        tracerRend.material.color = color;
        explosionRend.material.color = color;
    }

    //Al tocar el suelo, explotar y activar el area de cambio de color del reticulo
    private void OnCollisionEnter(Collision collision)
    {
        if (!exploded)
        {
            animator.SetBool("Explode", true);
            rb.isKinematic = true;
            StartCoroutine(reticle.Explode());
            tracer.Stop();
        }
    }
}
