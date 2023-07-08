using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAudioManager : MonoBehaviour
{
    [Header("Sound effects")]
    [SerializeField]
    private AudioSource soundEffectsSpeaker;
    [SerializeField]
    private AudioClip lobVenomCaskSoundClip;
    [SerializeField]
    private AudioClip contaminateSoundClip;
    [SerializeField]
    private AudioClip ambushStartupSoundClip;
    [SerializeField]
    private SideEffect noVialSideEffectSounds;


    [Header("VoiceOver")]
    [SerializeField]
    private AudioSource voiceSpeaker;

    [SerializeField]
    private AudioClip[] ambushStartupVoiceClips;
    [SerializeField]
    [Range(0f, 1f)]
    private float ambushStartupVoiceChance = 0.6f;

    [SerializeField]
    private AudioClip[] ambushBuffVoiceClips;
    [SerializeField]
    [Range(0f, 1f)]
    private float ambushBuffVoiceChance = 1f;

    [SerializeField]
    private AudioClip[] hurtVoiceEffects;
    [SerializeField]
    private AudioClip[] deathVoiceEffects;
    [SerializeField]
    private AudioClip deathImpactSoundEffect;

    [SerializeField]
    private AudioClip[] errorMessageVoiceover;



    // Start is called before the first frame update
    void Awake()
    {
        if (soundEffectsSpeaker == null) {
            Debug.LogError("No sound effect speaker found for the audio manager");
        }

        if (voiceSpeaker == null) {
            Debug.LogError("No voice speaker found for the audio manager");
        }
    }

    
    // Main function to play the launchPoisonBolt clip
    public void playLaunchPoisonBoltSound(SideEffect sideEffect = null) {
        soundEffectsSpeaker.clip = (sideEffect != null) ? sideEffect.getPrimaryAttackSound() : noVialSideEffectSounds.getPrimaryAttackSound();
        soundEffectsSpeaker.Play();
    }


    // Main function to play the launchPoisonBolt clip
    public void playLobCaskSound() {
        if (lobVenomCaskSoundClip == null) {
            Debug.LogWarning("No sound clip for lobbing a cask");
        }

        soundEffectsSpeaker.clip = lobVenomCaskSoundClip;
        soundEffectsSpeaker.Play();
    }


    // Main function to play the launchPoisonBolt clip
    public void playContaminateSound() {
        if (contaminateSoundClip == null) {
            Debug.LogWarning("No sound clip for contamination");
        }

        soundEffectsSpeaker.clip = contaminateSoundClip;
        soundEffectsSpeaker.Play();
    }


    // Main function to silence all voices
    public void silenceVoiceover() {
        voiceSpeaker.Stop();
    }


    // Main function to play stealth startup sounds
    public void playAmbushStartup() {
        // Sound effects
        if (ambushStartupSoundClip == null) {
            Debug.LogWarning("No sound effect clip for ambush startup");
        }

        soundEffectsSpeaker.clip = ambushStartupSoundClip;
        soundEffectsSpeaker.Play();

        // Voice
        playVoice(ambushStartupVoiceClips, ambushStartupVoiceChance);
    }


    // Main function to play death impact sound
    public void playDeathImpact() {
        soundEffectsSpeaker.clip = deathImpactSoundEffect;
        soundEffectsSpeaker.Play();
    }


    // Main function to play ambush buff sounds
    public void playAmbushBuff() {
        playVoice(ambushBuffVoiceClips, ambushBuffVoiceChance);
    }


    // Main function to play death sound effect
    public void playDeathVoice() {
        playVoice(deathVoiceEffects, 1f);
    }


    // Main function to play hurt sound effect
    public void playHurtVoice() {
        playVoice(hurtVoiceEffects, 1f);
    }


    // Main function to play error message voice
    public void playErrorMessageVoice() {
        playVoice(errorMessageVoiceover, 1f);
    }



    // Main private helper function to play voice clip
    //  Pre: voiceClipList != null && 0f <= voiceChance <= 1f. 
    //  Post: plays the sound effect if it's possible. If voiceClipList.Length == 0, do nothing and throw a warning
    private void playVoice(AudioClip[] voiceClipList, float voiceChance) {
        Debug.Assert(0f <= voiceChance && voiceChance <= 1f);
        Debug.Assert(voiceSpeaker != null);

        // If voice clip is invalid
        if (voiceClipList == null || voiceClipList.Length <= 0) {
            Debug.LogWarning("The associated voice clip is not initialized please check the audio manager");

        // Random chance - if triggered
        } else if (Random.Range(0f, 1f) <= voiceChance) {
            voiceSpeaker.clip = voiceClipList[Random.Range(0, voiceClipList.Length)];
            voiceSpeaker.Play();
        }
    }
}
