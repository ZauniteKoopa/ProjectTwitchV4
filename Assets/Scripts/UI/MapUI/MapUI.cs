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


    // Main function to recenter the dungeon layout
    public void recenter(Room[] dungeonLayout) {
        // Get the center of the MAP UI layout
        int mapRowCenter = mapRoomUnits.Length / 2;
        int mapColCenter = mapRoomUnits[0].rooms.Length / 2;

        // Get the center of the dungeon layout
        int layoutRowCenter = 0;
        int layoutColCenter = 0;

        foreach (Room room in dungeonLayout) {
            layoutRowCenter += room.mapRow;
            layoutColCenter += room.mapCol;
        }

        layoutRowCenter /= dungeonLayout.Length;
        layoutColCenter /= dungeonLayout.Length;

        // Translate room's center to map's center
        int distRowDelta = mapRowCenter - layoutRowCenter;
        int distColDelta = mapColCenter - layoutColCenter;
        foreach (Room room in dungeonLayout) {
            room.mapRow += distRowDelta;
            room.mapCol += distColDelta;
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
