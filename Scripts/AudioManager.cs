﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (SoundLibrary))]
public class AudioManager : MonoBehaviour
{
    public enum AudioChannel
    {
        Master, Sfx, Music
    };

    public float masterVolumePercent { get; private set; }
    public float sfxVolumePercent { get; private set; }
    public float musicVolumePercent { get; private set; }

    private AudioSource sfx2DSource;
    private AudioSource[] musicSources;
    private int activeMusicSourceIndex;

    public static AudioManager instance;

    private SoundLibrary soundLibrary;

    private Transform audioListener;
    private Transform playerTransform;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {

            instance = this;

            DontDestroyOnLoad(gameObject);

            soundLibrary = GetComponent<SoundLibrary>();

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music sources " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }

            GameObject newSfx2DSource = new GameObject("2D sfx source");
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            newSfx2DSource.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            if (!audioListener)
            {
                Debug.Log("Cant find audion listener!");
            }
            if (FindObjectOfType<Player>() != null)
            {
                playerTransform = FindObjectOfType<Player>().transform;
            }


            masterVolumePercent = PlayerPrefs.GetFloat("master vol", 1);
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", 1);
            sfxVolumePercent = PlayerPrefs.GetFloat("music vol", 1);
        }
    }

    private void Update()
    {
        //if (playerTransform != null)
            //audioListener.position = playerTransform.position;
    }

    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    public void PlaySound(AudioClip clip, Vector3 position)
    {
        AudioSource.PlayClipAtPoint(clip, position, sfxVolumePercent * masterVolumePercent);
    }

    public void PlaySound(string soundName, Vector3 position)
    {
        PlaySound(soundLibrary.GetClipFromName(soundName), position);
    }

    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(soundLibrary.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }

    IEnumerator AnimateMusicCrossfade(float duration)
    {
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);

            yield return null;
        }
    }

}
