using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager INSTANCE;

    private AudioSource audio;

    public AudioClip backgroundMusic;
    
    public AudioClip gameStart;
    public AudioClip gameOverVictory;
    public AudioClip gameOverDefeat;
    public AudioClip[] resourceSound;
    public AudioClip[] rockSounds;
    public AudioClip[] rootSounds;
    public AudioClip[] branchSounds;

    private readonly HashSet<SFX> sfxToPlay = new HashSet<SFX>();

    void Awake()
    {
        INSTANCE = this;
        audio = GetComponent<AudioSource>();
    }

    void LateUpdate()
    {
        foreach (var sfx in sfxToPlay)
        {
            PlayNow(sfx);
        }

        sfxToPlay.Clear();
    }

    public void StartBGM()
    {
        // audio.
        audio.Play();
    }

    public void Play(SFX sfx)
    {
        sfxToPlay.Add(sfx);
    }

    public void PlayNow(SFX sfx)
    {
        AudioClip clip = sfx switch
        {
            SFX.GAME_START => gameStart,
            SFX.GAME_OVER_VICTORY => gameOverVictory,
            SFX.GAME_OVER_DEFEAT => gameOverDefeat,
            SFX.HIT_RESOURCES => SelectRandom(resourceSound),
            SFX.HIT_ROCK => SelectRandom(rockSounds),
            SFX.ROOT_GROWTH => SelectRandom(rootSounds),
            SFX.BRANCH_GROWTH => SelectRandom(branchSounds),
            _ => null
        };

        if (clip)
        {
            audio.PlayOneShot(clip);
        }
        else
        {
            Debug.Log("no audio clip found for " + sfx);
        }
    }

    private AudioClip SelectRandom(AudioClip[] clips)
    {
        return clips[Random.Range(0, clips.Length)];
    }
}

public enum SFX
{
    GAME_START,
    GAME_OVER_VICTORY,
    GAME_OVER_DEFEAT,

    HIT_ROCK,
    ROOT_GROWTH,
    BRANCH_GROWTH,
    HIT_RESOURCES,
}