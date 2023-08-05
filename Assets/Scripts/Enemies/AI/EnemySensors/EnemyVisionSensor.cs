using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Main class for a simple enemy vision sensor in which the enemy just does radius vision
public class EnemyVisionSensor : MonoBehaviour
{
    // Main variables for vision
    [SerializeField]
    protected LayerMask visionMask;
    protected PlayerStatus nearbyTarget;

    // Main variables for short term memory
    [SerializeField]
    private float forgetDuration = 1f;
    private Coroutine forgetRoutine;

    // Main method to invoke events onto brain
    [SerializeField]
    protected IEnemyBehavior brain;
    [SerializeField]
    protected EnemyStatus enemyStatus;


    // On awake, error check
    private void Awake() {
        initialize();
    }


    // Main funtion to initialize
    protected virtual void initialize() {
        if (forgetDuration < 0f) {
            Debug.LogError("Forget duration for enemy sensor is negative. Should be zero or positive", transform);
        }

        if (brain == null) {
            Debug.LogError("Enemy Sensor is not connected to a behavior tree or AI behavior handler", transform);
        }
    }


    // On each fixed update frame, 
    private void Update() {
        // If in aggro state, do radius sensing
        if (brain.inAggroState()) {
            manageAggressiveSensing();
        } else {
            managePassiveSensing();
        }
    }


    // Main function to manage aggressive sensing each frame
    protected virtual void manageAggressiveSensing() {
        bool playerInRange = canSeePlayerInRange();

        // If player is in range and a forgetting sequence is occuring, stop forget sequence
        if (playerInRange && forgetRoutine != null) {
            StopCoroutine(forgetRoutine);
            forgetRoutine = null;
        
        // If player leaves range and no forget sequence is found, start forgetting
        } else if (!playerInRange && forgetRoutine == null) {
            forgetRoutine = StartCoroutine(forgetPlayerSequence());

        }
    }


    // Main function to manage passive sensing each frame
    protected virtual void managePassiveSensing() {
        bool seenPlayer = canSeePlayerInRange();

        if (seenPlayer) {
            brain.onSensedPlayer(nearbyTarget.transform);
        }
    }

    
    // Main private sequence to forget unit
    //  Post: sequence will force a delay between not seeing player and the brain forgetting the player exists. Updates any UI associated with this
    private IEnumerator forgetPlayerSequence() {
        yield return new WaitForSeconds(forgetDuration);

        // Forget player
        forgetPlayer();
    }


    // Main action of actually forgetting the player in terms of sensing
    protected virtual void forgetPlayer() {
        forgetRoutine = null;
        brain.onLostPlayer();
    }


    // Private helper function to see if you can actually see the unit
    //  Post: returns whether or not player is in sight range of enemy AND no objects are blocking
    protected virtual bool canSeePlayerInRange() {
        if (nearbyTarget == null) {
            return false;
        }

        // Get information for the ray: you can see the player if there are no barriers between player and enemy
        Vector3 targetPosition = nearbyTarget.transform.position;
        Vector3 rayDir = targetPosition - transform.position;
        float rayDist = rayDir.magnitude;
        rayDir.Normalize();
        bool seePlayer = !Physics.Raycast(transform.position, rayDir, rayDist, visionMask);

        // If you can see the player, check if the player is visible to consider invisibility
        if (seePlayer) {
            seePlayer = nearbyTarget.canSeePlayer(enemyStatus);
        }

        // Return whether or not ray cast dir met any barriers
        return seePlayer;
    }


    // Event handler function for when player has entered the sense box
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus testPlayer = collider.GetComponent<PlayerStatus>();

        if (testPlayer != null) {
            nearbyTarget = testPlayer;
        }

        onTriggerEnterExt(collider);
    }

    protected virtual void onTriggerEnterExt(Collider collider) {}


    // Event handler function for when player has exited the sense box
    private void OnTriggerExit(Collider collider) {
        PlayerStatus testPlayer = collider.GetComponent<PlayerStatus>();

        if (testPlayer != null) {
            nearbyTarget = null;
        }

        onTriggerExitExt(collider);
    }

    protected virtual void onTriggerExitExt(Collider collider) {}

}
