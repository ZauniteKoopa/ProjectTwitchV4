using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyWave : MonoBehaviour
{
    public UnityEvent waveFinishedEvent;

    private List<EnemyStatus> enemies;
    private int numEnemiesLeft = 0;
    private bool finished = false;
    private bool activated = false;


    // Main function to initialize wave
    //  Pre: tries to find all enemies that are under this EnemyWave object as a child, MUST HAVE AT LEAST 1 ENEMY AS A CHILD
    //  Post: all enemies are connected to this enemy wave object
    private void Start() {
        // Iterate through all of the children
        enemies = new List<EnemyStatus>();

        for (int c = 0; c < transform.childCount; c++) {
            EnemyStatus curEnemy = transform.GetChild(c).GetComponent<EnemyStatus>();
            if (curEnemy != null) {
                numEnemiesLeft++;
                curEnemy.deathEvent.AddListener(onEnemyDeath);
                enemies.Add(curEnemy);
            }

            transform.GetChild(c).gameObject.SetActive(false);
        }


        // Error check
        if (numEnemiesLeft <= 0) {
            Debug.LogError("NO ENEMIES FOUND AS A CHILD OF ENEMY WAVE OBJECT");
        }
    }


    // Main function to activate the wave
    public void activate() {
        if (!activated) {
            activated = true;

            foreach (EnemyStatus enemy in enemies) {
                //  TO-DO: change this to just spawn in an enemy gradually and not just make something pop up immediately
                enemy.gameObject.SetActive(true);
            }
        }
    }



    // Event handler function for when an enemy dies
    private void onEnemyDeath() {
        numEnemiesLeft--;

        if (numEnemiesLeft <= 0 && !finished) {
            finished = true;
            waveFinishedEvent.Invoke();
        }
    }

}
