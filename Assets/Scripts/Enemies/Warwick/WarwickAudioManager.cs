using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarwickAudioManager : MonoBehaviour
{
    [Header("Speakers")]
    [SerializeField]
    private AudioSource voiceSpeaker;
    [SerializeField]
    private AudioSource sfxSpeaker;
    [SerializeField]
    private AudioSource howlSpeaker;

    [Header("Boss Components to Listen To")]
    [SerializeField]
    private WarwickPassiveBranch passiveBranch;
    [SerializeField]
    private WarwickAggroBranch aggroBranch;
    [SerializeField]
    private BossEnemyStatus warwickStatus;

    [Header("Voice Audio Clips")]
    [SerializeField]
    private AudioClip[] bloodHuntPlayerVoice;
    [SerializeField]
    private AudioClip[] bloodHuntEnemyVoice;
    [SerializeField]
    private AudioClip[] phaseTransitionVoice;
    [SerializeField]
    private AudioClip[] lungeVoice;
    [SerializeField]
    private AudioClip[] deathVoice;

    [Header("Sound Effects Clips")]
    [SerializeField]
    private AudioClip[] howlCastSounds;
    [SerializeField]
    private AudioClip[] howlReleaseSounds;
    [SerializeField]
    private AudioClip[] howlBrokenSounds;
    [SerializeField]
    private AudioClip[] lungeSounds;
    [SerializeField]
    private AudioClip[] slashSounds;
    [SerializeField]
    private AudioClip[] transitionSounds;



    // On awake, connect to events
    private void Awake() {
        // Connect to all events relating to enemy status
        warwickStatus.enemyPhaseTransitionBeginEvent.AddListener(onPhaseTransition);

        // Connect to all events relating to Passive Branch
        passiveBranch.bloodHuntStartEvent.AddListener(onBloodHuntTriggered);

        // Connect to all events relating to aggro branch
        aggroBranch.howlStunBeginEvent.AddListener(onHowlFailure);
        aggroBranch.howlSuccess.AddListener(onHowlSuccess);
        aggroBranch.howlStart.AddListener(onHowlAttackStart);

        aggroBranch.lungeAttack.AddListener(onLungeAttackStart);
        aggroBranch.slashAttack.AddListener(onSlashAttackSoundStart);
    }



    // Main event handler function for when blood hunt is triggered
    public void onBloodHuntTriggered() {
        AudioClip[] curVoice = (passiveBranch.isBloodiedTargetPlayer()) ? bloodHuntPlayerVoice : bloodHuntEnemyVoice;
        playRandomClip(voiceSpeaker, curVoice);
    }


    // Main event handler function for when unit dies
    public void onDeath() {
        voiceSpeaker.transform.parent = null;
        playRandomClip(voiceSpeaker, deathVoice);
    }


    // Main event handler function for when phase transition starts
    public void onPhaseTransition() {
        playRandomClip(voiceSpeaker, phaseTransitionVoice);
        playRandomClip(sfxSpeaker, transitionSounds);
    }


    // Main event handler function for when lunge starts
    public void onLungeAttackStart() {
        playRandomClip(voiceSpeaker, lungeVoice);
        playRandomClip(sfxSpeaker, lungeSounds);
    }


    // Main event handler for when slash attacks start
    public void onSlashAttackSoundStart() {
        playRandomClip(sfxSpeaker, slashSounds);
    }


    // Main event handler for when Howl attack starts
    public void onHowlAttackStart() {
        playRandomClip(howlSpeaker, howlCastSounds);
    }


    // Main event handler for when how attack succeeds
    public void onHowlSuccess() {
        playRandomClip(howlSpeaker, howlReleaseSounds);
    }


    // Main event handler for when howl attack fails
    public void onHowlFailure() {
        playRandomClip(sfxSpeaker, howlBrokenSounds);
    }


    // Main private helper function to play a random clip with a speaker
    private void playRandomClip(AudioSource speaker, AudioClip[] clips) {
        if (clips.Length > 0) {
            Debug.Assert(speaker != null);

            speaker.clip = clips[Random.Range(0, clips.Length)];
            speaker.Play();
        }
    }
}
