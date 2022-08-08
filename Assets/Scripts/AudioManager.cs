using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using Sirenix.OdinInspector;
using UnityEditor;

public class AudioManager : SingletonMono<AudioManager>
{
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SOUND_ENUM_NAME = "SoundId";

    public AudioMixer mixer;
    public AudioMixerGroup sfxMixer;
    public AudioMixerGroup musicMixer;

    //Array de sonidos del juego
    [HideReferenceObjectPicker][SerializeField]
    public List<Sound> sounds;

    [Range(0.000001f, 1)]
    public float sfxVolume = 1;
    [Range(0.000001f, 1)]
    public float musicVolume = 1;


    public float paintPartSpeedSound = 5;


    protected override void Awake()
    {
        base.Awake();
        foreach(Sound s in sounds)
        {
            GameObject go = new GameObject(s.name);
            go.transform.SetParent(transform);
            s.source = go.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;
            s.source.spatialBlend = 0; //Sonidos 2D
            s.source.outputAudioMixerGroup = (s.mixer == Sound.MixerType.SFX) ? sfxMixer : musicMixer;
            s.originalPitch = s.pitch; 
        }
    }

    [ExecuteAlways]
    public void OnValidate()
    {
        UpdateMixerValues();
    }

    [Button][ExecuteAlways]
    public void UpdateSoundsEnum()
    {
        List<string> values = new List<string>();

        foreach (Sound s in sounds)
        {
            if(values.Contains(s.name))
            {
                Debug.LogError("Sonido "+ s.name +" repetido. No se ha creado el enumerado");
                return;
            }

            values.Add(s.name);
        }

        EnumGenerator.GenerateEnum(SOUND_ENUM_NAME, values);
    }


    #region METODOS SONIDOS

    public Sound PlayOverriding(SoundId name)
    {
        Sound s = sounds.Find(sound => sound.name.ToEnumFormat() == name.ToString());
        if (s.source.isPlaying)
            s.source.Stop();
        CustomPlay(s, false);

        return s;
    }

    public Sound PlayAdditively(SoundId name)
    {
        Sound s = sounds.Find(sound => sound.name.ToEnumFormat() == name.ToString());
        CustomPlay(s, true);

        return s;
    }

    public Sound PlayAdditivelyWithOffset(SoundId name, float offset)
    {
        Sound s = sounds.Find(sound => sound.name.ToEnumFormat() == name.ToString());
        if(CustomPlay(s, true))
            _ = s.StartPlayingOffset(offset);

        return s;
    }

    public Sound PlayInNewSource(SoundId name, out AudioSource source)
    {
        Sound sound = sounds.Find(sound => sound.name.ToEnumFormat() == name.ToString());

        source = new GameObject("Source aux ("+sound.name+")").AddComponent<AudioSource>();
        source.CopyFrom(sound.source);

        //Variacion de pitch cada vez que se llama para evitar monotonia
        if (sound.pitchRandMaxOffset > 0)
        {
            float variation = UnityEngine.Random.Range(-sound.pitchRandMaxOffset, sound.pitchRandMaxOffset);
            source.pitch = sound.originalPitch + variation;
        }

        source.PlayOneShot(sound.clip);

        return sound;
    }


    private bool CustomPlay(Sound sound, bool additively)
    {
        if(sound.playable)
        {
            //Variacion de pitch cada vez que se llama para evitar monotonia
            if (sound.pitchRandMaxOffset>0)
            {
                float variation = UnityEngine.Random.Range(-sound.pitchRandMaxOffset, sound.pitchRandMaxOffset);
                sound.source.pitch = sound.originalPitch + variation;
            }

            //Reproducimos parando o no el sonido que se esta reproduciendo
            if (additively)
                sound.source.PlayOneShot(sound.clip);
            else
                sound.source.Play();

            return true;
        }

        return false;
    }

    public void Stop(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        s.source.Stop();
    }

    #endregion


    #region MIXER

    public void UpdateMixerValues()
    {
        //Pasamos el parametro al rango en dB que comprenda de 0 a 1
        SetVolume(SFX_VOLUME_KEY, sfxVolume);
        SetVolume(MUSIC_VOLUME_KEY, musicVolume);
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Cambiar el sonido con un valor de 0 a 1 (pasandolo a decibelios exponencialemente)
    /// </summary>
    /// <param name="volumeKey"></param>
    /// <param name="value01"></param>
    public void SetVolume(string volumeKey, float value01)
    {
        mixer.SetFloat(volumeKey, Mathf.Log10(value01) * 20);
    }

    #endregion
}
