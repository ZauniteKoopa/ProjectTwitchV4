using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchInventory : MonoBehaviour
{
    // Poison Vial Info
    [SerializeField]
    private SideEffect startingSideEffect;
    private PoisonVial primaryVial;
    private PoisonVial secondaryVial;

    // Cooldown info
    [SerializeField]
    [Min(0.1f)]
    private float contaminateCooldown = 8f;
    [SerializeField]
    [Min(0.1f)]
    private float ambushCooldown = 12f;
    private Coroutine runningCaskCooldownSequence = null;
    private Coroutine runningContaminateCooldownSequence = null;
    private Coroutine runningAmbushCooldownSequence = null;

    // UI
    [SerializeField]
    private PlayerScreenUI screenUI;


    // Start is called before the first frame update
    void Awake()
    {
        resetScreenUI();

        primaryVial = new PoisonVial(PoisonVialStat.POTENCY, startingSideEffect);
        primaryVial.contaminateExecuteEvent.AddListener(onAmbushReset);
        screenUI.displayPrimaryVial(primaryVial);
    }


    // Main function to reset ScreenUI variables: cooldown status and vial status
    private void resetScreenUI() {
        screenUI.displayAmbushCooldown(1f);
        screenUI.displayContaminateCooldown(1f);
        screenUI.displayCaskCooldown(1f);

        screenUI.displayPrimaryVial(null);
        screenUI.displaySecondaryVial(null);
    }


    // Main function to check if you're carrying a vial or not
    public bool carryingPrimaryVial() {
        return primaryVial != null;
    }



    // ---------------------------------------
    //  PRIMARY ATTACK
    // ---------------------------------------


    // Main function to get primary variable frame data
    public int getPrimaryStartFrame() {
        return primaryVial.getPrimaryAttackStartFrames();
    }


    // Main function to get primary variable frame data
    public int getPrimaryEndFrame() {
        return primaryVial.getPrimaryAttackEndFrames();
    }


    // Main function to fire the primary vial if it's possible
    //  Pre: the attackDirection is the direction of attack
    //  Post: returns true if successful. false otherwise
    public bool firePrimaryBolt(Vector3 attackDir, Transform attacker) {
        if (primaryVial == null) {
            return false;
        }

        // Fire bullet and then check ammo afterwards
        bool success = primaryVial.firePrimaryAttack(attackDir, attacker);
        if (success && primaryVial.getAmmo() <= 0) {
            primaryVial = null;
        }

        screenUI.displayPrimaryVial(primaryVial);
        return success;
    }


    // ---------------------------------------
    //  SECONDARY ATTACK
    // ---------------------------------------


    // Main function to get secondary variable frame data
    public int getSecondaryAttackStartFrames() {
        return primaryVial.getSecondaryAttackStartFrames();
    }


    // Main function to get primary variable frame data
    public int getSecondaryAttackEndFrames() {
        return primaryVial.getSecondaryAttackEndFrames();
    }


    // Main function to fire the primary vial if it's possible
    //  Pre: the attackDirection is the direction of attack
    //  Post: returns true if successful. false otherwise
    public bool fireSecondaryLob(Vector3 tgtPos, Transform attacker) {
        if (primaryVial == null && runningCaskCooldownSequence != null) {
            return false;
        }

        // Fire bullet and then check ammo afterwards
        bool success = primaryVial.fireSecondaryAttack(tgtPos, attacker);
        if (success) {
            runningCaskCooldownSequence = StartCoroutine(caskCooldownSequence(primaryVial.getSecondaryAttackCooldown()));

            if (primaryVial.getAmmo() <= 0) {
                primaryVial = null;
            }
        }

        screenUI.displayPrimaryVial(primaryVial);
        return success;
    }


    // Main function to check if you can actually fire secondary attack
    public bool canFireSecondaryLob() {
        return primaryVial != null && primaryVial.canFireSecondaryLob() && runningCaskCooldownSequence == null;
    }

    
    // Main function to do the secondary vial cooldown sequence
    //  Pre: curCooldown of the cask. > 0f
    private IEnumerator caskCooldownSequence(float curCooldown) {
        Debug.Assert(curCooldown > 0f);

        // Setup
        float timer = 0f;
        screenUI.displayCaskCooldown(0f);

        // Loop
        while (timer < curCooldown) {
            yield return 0;
            timer += Time.deltaTime;

            screenUI.displayCaskCooldown(timer / curCooldown);
        }

        runningCaskCooldownSequence = null;
    }



    // ---------------------------------------
    //  Contaminate
    // ---------------------------------------


    // Main sequence for the cooldown sequence for contamination
    private IEnumerator contaminateCooldownSequence() {
        float timer = 0f;
        screenUI.displayContaminateCooldown(0f);

        while (timer <= contaminateCooldown) {
            yield return 0;
            timer += Time.deltaTime;

            screenUI.displayContaminateCooldown(timer / contaminateCooldown);
        }

        runningContaminateCooldownSequence = null;
    }


    // Main public function to check if you can conatminate (not on cooldown)
    public bool canContaminate() {
        return runningContaminateCooldownSequence == null;
    }


    // Main public function to activate contamination cooldown
    //  If already activated, don't do anything
    public void activateContaminationCooldown() {
        if (runningContaminateCooldownSequence == null) {
            runningContaminateCooldownSequence = StartCoroutine(contaminateCooldownSequence());
        }
    }



    // ---------------------------------------
    //  Ambush
    // ---------------------------------------


    // Main sequence for the cooldown sequence for contamination
    private IEnumerator ambushCooldownSequence() {
        float timer = 0f;
        screenUI.displayAmbushCooldown(0f);

        while (timer <= ambushCooldown) {
            yield return 0;
            timer += Time.deltaTime;

            screenUI.displayAmbushCooldown(timer / ambushCooldown);
        }

        runningAmbushCooldownSequence = null;
    }


    // Main public function to check if you can conatminate (not on cooldown)
    public bool canAmbush() {
        return runningAmbushCooldownSequence == null;
    }


    // Main public function to activate contamination cooldown
    //  If already activated, don't do anything
    public void activateAmbushCooldown() {
        if (runningAmbushCooldownSequence == null) {
            runningAmbushCooldownSequence = StartCoroutine(ambushCooldownSequence());
        }
    }


    // Main event handler function to reset ambush cooldown
    private void onAmbushReset() {
        if (runningAmbushCooldownSequence != null) {
            StopCoroutine(runningAmbushCooldownSequence);
            runningAmbushCooldownSequence = null;

            screenUI.displayAmbushCooldown(1f);
        }
    }



    // ---------------------------------------
    //  Vial and Ingredient Management
    // ---------------------------------------


    // Main function to swap vials
    public void swapVials() {
        PoisonVial tempVial = primaryVial;
        primaryVial = secondaryVial;
        secondaryVial = tempVial;

        screenUI.displayPrimaryVial(primaryVial);
        screenUI.displaySecondaryVial(secondaryVial);
    }
    
}
