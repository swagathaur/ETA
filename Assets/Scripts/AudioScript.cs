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

    AudioSource audioPlayer;
    public AudioClip levelAudio;
    public List<AudioClip> baseAudioClips;
    public List<AudioClip> player2AudioClips;
    public List<AudioClip> player1AudioClips;

    public void playSound(baseAudio audioClipToPlay)
    {
        audioPlayer.PlayOneShot(baseAudioClips[(int)audioClipToPlay], SFXVolume);
    }
    public void playSound(playerAudio audioClipToPlay, PlayerIndex index)
    {
        if (index == PlayerIndex.One)
        {
            audioPlayer.PlayOneShot(player1AudioClips[(int)audioClipToPlay], VoiceVolume);
        }
        else
        {
            audioPlayer.PlayOneShot(player1AudioClips[(int)audioClipToPlay], VoiceVolume);
        }
    }

    public void PlayBGM(AudioClip musicToUse = null)
    {
        if (musicToUse == null)
            audioPlayer.clip = levelAudio;
        audioPlayer.clip = musicToUse;

        audioPlayer.Play();
        audioPlayer.loop = true;
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
