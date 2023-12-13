using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRoomUI : MonoBehaviour
{
    [Header("Reference variables")]
    [SerializeField]
    private Image baseRoomImage;
    [SerializeField]
    private Image westWall;
    [SerializeField]
    private Image eastWall;
    [SerializeField]
    private Image northWall;
    [SerializeField]
    private Image southWall;
    [SerializeField]
    private Image enemyIndicator;
    [SerializeField]
    private GameObject[] roomCorners;

    // Enemy thresholds will MAP directly with output indicator scale. Please make sure the indexing matches
    [Header("Enemy Indicator Management")]
    [SerializeField]
    private int maxEnemyThreshold = 5;
    [SerializeField]
    [Range(0f, 20f)]
    private float minIndicatorSize = 5f;
    [SerializeField]
    [Range(0f, 20f)]
    private float maxIndicatorSize = 15f;



    // Player indicator management
    [Header("Room color mapping")]
    [SerializeField]
    private Color playerInsideColor = Color.green;
    [SerializeField]
    private Color battleRoomColor = Color.red;
    [SerializeField]
    private Color keyRoomColor = Color.cyan;
    [SerializeField]
    private Color spawnRoomColor = Color.yellow;
    private Color defaultColor;


    // On awake, set up
    private void Awake() {
        defaultColor = baseRoomImage.color;
    }


    // Main function to display a room
    public void displayRoom(Room room) {
        Debug.Assert(room != null);

        gameObject.SetActive(true);

        // Update room image body and player indicator
        baseRoomImage.enabled = room.revealedOnMap();
        foreach (GameObject corner in roomCorners) {
            corner.SetActive(room.revealedOnMap());
        }

        baseRoomImage.color = getRoomColor(room);

        // Update walls
        westWall.enabled = (room.revealedOnMap()) ? !room.westOpen : false;
        eastWall.enabled = (room.revealedOnMap()) ? !room.eastOpen : false;
        northWall.enabled = (room.revealedOnMap()) ? !room.northOpen : false;
        southWall.enabled = (room.revealedOnMap()) ? !room.southOpen : false;

        // Update enemy indicator
        enemyIndicator.enabled = (room.getNumEnemiesInside() > 0);
        if (room.getNumEnemiesInside() > 0) {
            float t = (float)(room.getNumEnemiesInside() - 1) / (float)(maxEnemyThreshold - 1);
            float curIndicatorScale = Mathf.Lerp(minIndicatorSize, maxIndicatorSize, t);
            enemyIndicator.GetComponent<RectTransform>().sizeDelta = curIndicatorScale * Vector2.one;
        }
    }


    // Main function to display an empty inactive room
    public void displayEmpty() {
        gameObject.SetActive(false);
    }


    // Main helper function to get the color of the room
    public Color getRoomColor(Room room) {
        if (room.playerInside) {
            return playerInsideColor;

        } else if (room is BattleRoom) {
            return battleRoomColor;

        } else if (room is SpawnRoom) {
            return spawnRoomColor;

        } else if (room is KeyRoom) {
            return keyRoomColor;

        } else {
            return (room.adjacentToBattleRoom) ? battleRoomColor : defaultColor;
        }
    }
}
