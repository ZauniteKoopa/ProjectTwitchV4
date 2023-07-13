using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DungeonFloorLayout
{
    private Room[] dungeonRooms;
    private Room finalBattleRoom;


    // Main function to intialize DungeonFloorLayout
    //  Pre: rooms and battle room are non-null, the battle room can be found in rooms
    public DungeonFloorLayout(Room[] rooms, Room battleRoom) {
        Debug.Assert(rooms.Length > 1);
        Debug.Assert(battleRoom != null);

        bool foundBattleRoom = false;
        foreach (Room room in rooms) {
            Debug.Assert(room != null);

            if (room == battleRoom) {
                foundBattleRoom = true;
            }
        }

        Debug.Assert(foundBattleRoom);
        dungeonRooms = rooms;
        finalBattleRoom = battleRoom;
    }


    // Main function to get a random position in the dungeon
    public Vector3 getRandomPosition() {
        // Randomly choose a room
        Debug.Assert(dungeonRooms.Length > 0);
        Room curRoom = dungeonRooms[Random.Range(0, dungeonRooms.Length)];
        while (curRoom == finalBattleRoom) {
            curRoom = dungeonRooms[Random.Range(0, dungeonRooms.Length)];
        }

        Debug.Assert(curRoom != null);

        // Get spawn position
        float emptySpaceLength = Room.ROOM_SIZE - Room.WALL_OFFSET;
        Vector3 spawnPos = new Vector3(Random.Range(-emptySpaceLength / 2f, emptySpaceLength / 2f), 0f, Random.Range(-emptySpaceLength / 2f, emptySpaceLength / 2f));
        spawnPos += curRoom.transform.position;

        // Get nav mesh adjusted point 
        NavMeshHit hitInfo;
        NavMesh.SamplePosition(spawnPos, out hitInfo, 10f, NavMesh.AllAreas);
        spawnPos = hitInfo.position;

        return spawnPos;
    }
}
