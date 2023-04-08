using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource soundEffectsSpeaker;
    [SerializeField]
    private AudioClip[] launchPoisonBoltClips;
    [SerializeField]
    private AudioClip lobVenomCaskSoundClip;

    // Start is called before the first frame update
    void Awake()
    {
        if (soundEffectsSpeaker == null) {
            Debug.LogError("No sound effect speaker found for the audio manager");
        }
    }

    
    // Main function to play the launchPoisonBolt clip
    public void playLaunchPoisonBoltSound() {
        if (launchPoisonBoltClips == null || launchPoisonBoltClips.Length <= 0) {
            Debug.LogWarning("No sound clip for launching a poison bolt");
        }

        soundEffectsSpeaker.clip = launchPoisonBoltClips[Random.Range(0, launchPoisonBoltClips.Length)];
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
}
