using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameGlobals;
using System.Linq;
using DG.Tweening;

//Habilidad de eliminar un color selectivamente en una zona amplia
public class SkillBlackBomb : Skill
{
    public const float finalScale = 16;
    public const float duration = 2f;
    private GameObject wavesObject;
    //private ParticleSystem wavesParticles;
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
        Vector3 pos = GameGlobals.Cursor;
        wavesObject.transform.position = pos;
        //wavesParticles = wavesObject.GetComponentInChildren<ParticleSystem>(true);
        //wavesCollider = wavesObject.GetComponentInChildren<Collider>(true);
        //StartCoroutine(DoBombWaveEffect());

        wavesObject.transform.DOScale(finalScale, duration).ChangeStartValue(Vector3.zero).SetEase(Ease.OutSine);
        Material material = wavesObject.GetComponent<MeshRenderer>().material;
        material.DOFade(0f, duration).SetEase(Ease.InSine);
        material.DOFloat(2f, "Tiling", duration).ChangeStartValue(0.5f).OnComplete(() => Destroy(wavesObject)).SetEase(Ease.InQuad);

        AudioManager.self.PlayAdditively(SoundId.Black_bomb);
        CameraMotion.self.ShakeCamera(duration);
    }

    public IEnumerator DoBombWaveEffect()
    {

        //wavesParticles.Play();
        //wavesCollider.enabled = false;
        yield return new WaitForSeconds(0.3f);
        //wavesCollider.enabled = true;
        //yield return new WaitForSeconds(1.5f);
        //wavesCollider.enabled = false;
    }
}