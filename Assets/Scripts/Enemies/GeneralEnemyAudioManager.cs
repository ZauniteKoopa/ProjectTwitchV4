using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralEnemyAudioManager : MonoBehaviour
{
    [Header("Sound effects")]
    [SerializeField]
    private AudioSource soundEffectsSpeaker;
    [SerializeField]
    private AudioClip attackSoundEffect;


    [Header("VO")]
    [SerializeField]
    private AudioSource voiceSpeaker;
    [SerializeField]
    private AudioClip deathVoiceOver;



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


    // Main function to play attack sound effect
    public void playAttackSoundEffect() {
        if (attackSoundEffect == null) {
            Debug.LogWarning("No sound clip for lobbing a cask");
        }

        soundEffectsSpeaker.clip = attackSoundEffect;
        soundEffectsSpeaker.Play();
    }


    // Main function to play death sound
    public void playDeathSoundEffect() {
        voiceSpeaker.transform.parent = null;

        if (deathVoiceOver == null) {
            Debug.LogWarning("No sound clip for lobbing a cask");
        }

        voiceSpeaker.clip = deathVoiceOver;
        voiceSpeaker.Play();
    }
}
