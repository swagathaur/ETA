using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using XInputDotNetPure;

public enum baseAudio
{
    CLIP_COUNTER_NORMAL = 0,
    CLIP_COUNTER_PERFECT,
    CLIP_DASH,
    CLIP_FOOTSTEP,
};

public enum playerAudio
{ 
    CLIP_HIT = 0,
    CLIP_JUMP,
    CLIP_ATTACK,
    CLIP_TAUNT
}

public class AudioScript : MonoBehaviour
{
    public float SFXVolume;
    public float VoiceVolume;
    public float MusicVolume;

    [HideInInspector]
    public AudioClip levelAudio;

    public List<AudioClip> baseAudioClips;
    public List<AudioClip> player2AudioClips;
    public List<AudioClip> player1AudioClips;

    private AudioSource audioPlayer;

    public void playSound(baseAudio audioClipToPlay)
    {
        audioPlayer.PlayOneShot(baseAudioClips[(int)audioClipToPlay], SFXVolume * (1 / audioPlayer.volume));
    }
    public void playSound(playerAudio audioClipToPlay, PlayerIndex index)
    {
        if (index == PlayerIndex.One)
        {
            audioPlayer.PlayOneShot(player1AudioClips[(int)audioClipToPlay], SFXVolume * (1 / audioPlayer.volume));
        }
        else
        {
            audioPlayer.PlayOneShot(player1AudioClips[(int)audioClipToPlay], SFXVolume * (1 / audioPlayer.volume));
        }
    }
    public void playSound(AudioClip audioClipToPlay)
    {
        audioPlayer.PlayOneShot(audioClipToPlay, VoiceVolume * (1 / audioPlayer.volume));
    }
    public void playSound(AudioClip audioClipToPlay, float Volume)
    {
        audioPlayer.PlayOneShot(audioClipToPlay, VoiceVolume * (1 / audioPlayer.volume));
    }

    public void StopSound()
    {
        PlayBGM(Resources.Load("BLANK") as AudioClip);
    }

    public void PlayBGM(AudioClip musicToUse = null)
    {
        if (musicToUse == null)
            audioPlayer.clip = levelAudio;
        else
            audioPlayer.clip = musicToUse;

        audioPlayer.volume = MusicVolume;
        audioPlayer.loop = true;
        audioPlayer.Play();
    }

    public void changeVolume(float volume)
    {
        audioPlayer.volume = volume;
    }

    void Start()
    {
        audioPlayer = GetComponent<AudioSource>();
    }
}
