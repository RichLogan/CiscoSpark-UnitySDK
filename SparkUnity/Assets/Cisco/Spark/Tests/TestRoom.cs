using UnityEngine;
using Cisco.Spark;

public class TestRoom : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        CreateRoom();
    }

    void CreateRoom()
    {
        // Create local Room.
        var room = new Room("Unity SDK Test Room", null);
        StartCoroutine(room.Commit(error =>
        {
            // Error on Commit.
            Debug.LogError(error.Message);
        }, success =>
        {
            // Move onto loading Room.
            var searchedRoom = new Room(room.Id);
            StartCoroutine(searchedRoom.Load(error =>
            {
                // Error on Load.
                Debug.LogError(error.Message);
            }, searchedRoomSuccess =>
            {
                if (searchedRoom.Title == room.Title)
                {
                    // Load Room proves it's passed here.
                    Debug.Log("Saving new Room to Spark passed!");
                    LoadRoom(searchedRoom);
                }
            }));
        }));
    }

    void LoadRoom(Room room)
    {
        // This has already proved it's passed if it's made it this far
        // from the create check.
        Debug.Log("Loading Room from ID passed!");
        UpdateRoom(room);
    }

    void UpdateRoom(Room room)
    {
        room.Title = "Unity SDK Test Room - Updated";
        StartCoroutine(room.Commit(error =>
        {
            Debug.Log(room.Title);
            // Error on Update.
            Debug.LogError(error.Message);
        }, success =>
        {
            var checkUpdatedRoom = new Room(room.Id);
            StartCoroutine(checkUpdatedRoom.Load(error =>
            {
                Debug.LogError(error.Message);
            }, updateSuccess =>
            {
                if (checkUpdatedRoom.Title == room.Title)
                {
                    Debug.Log("Update Room passed!");
                    DeleteRoom(checkUpdatedRoom);
                }
            }));
        }));
    }

    void DeleteRoom(Room room)
    {
        StartCoroutine(room.Delete(error =>
        {
            Debug.Log(error.Message);
        }, success =>
        {
            // Check room was deleted successfully.
            var checkDeletedRoom = new Room(room.Id);
            StartCoroutine(room.Load(error =>
            {
                Debug.Log("Delete Room passed!");
                ListRooms();
            }, updateSuccess =>
            {
                Debug.LogError("Delete Room failed!");
            }));
        }));
    }

    void ListRooms()
    {
        StartCoroutine(Room.ListRooms(error =>
        {
            Debug.Log(error.Message);
        }, results =>
        {
            if (results.Count > 0)
            {
                Debug.Log("List Rooms Passed!");
            }
        }));
    }
}
