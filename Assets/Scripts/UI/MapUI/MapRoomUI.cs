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
    [SerializeField]
    private Color playerInsideColor = Color.yellow;
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
        baseRoomImage.color = (room.playerInside) ? playerInsideColor : defaultColor;

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
}
