using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RoomRow {
    public MapRoomUI[] rooms;
}

public class MapUI : MonoBehaviour
{
    [SerializeField]
    private RoomRow[] mapRoomUnits;

    public static MapUI mainMapUI;


    // On awake., set this map UI as the new map UI (ONLY 1 MAP UI PER SCENE)
    private void Awake() {
        MapUI[] foundMapUI = FindObjectsOfType<MapUI>();
        if (foundMapUI.Length > 1) {
            Debug.LogError("2 MAP UIS FOUND IN THE CURRENT SCENE");
        }

        mainMapUI = this;
    }

    
    // Main function to render all rooms on the map
    public void render(Room[] dungeonLayout) {
        // Go through all the rooms in the grid
        for (int r = 0; r < mapRoomUnits.Length; r++) {
            for (int c = 0; c < mapRoomUnits[r].rooms.Length; c++) {
                Room matchingRoom = findGridRoom(r, c, dungeonLayout);

                if (matchingRoom != null) {
                    mapRoomUnits[r].rooms[c].displayRoom(matchingRoom);
                } else {
                    mapRoomUnits[r].rooms[c].displayEmpty();
                }
            }
        }
    }


    // Main function to find a grid coordinate in a list of rooms
    //  Pre: 0 <= r < mapRoomUnits.Length, 0 <= c < mapRoomUnits[r].rooms.Length, dungeonLayout != null
    //  Post: returns a non-null room if its found. otherwise, return null
    private Room findGridRoom(int r, int c, Room[] dungeonLayout) {
        foreach (Room room in dungeonLayout) {
            if (room.mapRow == r && room.mapCol == c) {
                return room;
            }
        }

        return null;
    }
}
