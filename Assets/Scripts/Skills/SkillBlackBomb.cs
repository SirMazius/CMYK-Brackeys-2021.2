using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;
using System.Linq;
using DG.Tweening;

//Habilidad de eliminar un color selectivamente en una zona amplia
public class SkillBlackBomb : Skill
{
    public const float radius = 5f;
    private GameObject wavesObject;
    private ParticleSystem wavesParticles;
    private Collider wavesCollider;


    public void Awake()
    {
        Type = SkillType.BLACK_BOMB;
    }

    public override void Launch()
    {
        base.Launch();

        //TODO:CAMBIAR LAZAMIENTO
        //var moninkers = FindObjectsOfType<MoninkerController>();
        //foreach(var m in moninkers)
        //{
        //    if (m.MoninkerColor == InkColorIndex.BLACK)
        //        GameManager.self.DeactivateMoninker(m);
        //}

        wavesObject = Instantiate(GameManager.self.BlackBombWavesPrefab);
        wavesObject.transform.position = GameGlobals.Cursor;
        wavesParticles = wavesObject.GetComponentInChildren<ParticleSystem>(true);
        StartCoroutine(DoBombWaveEffect());

        AudioManager.self.PlayAdditively(SoundId.Black_bomb);
        CameraMotion.self.ShakeCamera();
    }

    public IEnumerator DoBombWaveEffect()
    {
        wavesParticles.Play();
        wavesCollider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        wavesCollider.enabled = true;
        yield return new WaitForSeconds(1.5f);
        wavesCollider.enabled = false;
    }
}