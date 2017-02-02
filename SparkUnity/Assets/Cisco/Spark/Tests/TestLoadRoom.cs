using UnityEngine;
using Cisco.Spark;

public class TestLoadRoom : MonoBehaviour
{
    void Start()
    {
        SetUp();
    }

    void SetUp()
    {
        var room = new Room("Unity SDK Test Room", null);
        StartCoroutine(room.Commit(error =>
        {
            // Error on Commit.
            IntegrationTest.Fail("Failed to create the test room" + error.Message);
        }, success =>
        {
            Test(room);
        }));
    }

    void Test(Room room)
    {
        // Need to load 
        var searchedRoom = new Room(room.Id);
        StartCoroutine(searchedRoom.Load(error =>
        {
            // Error on Load.
            IntegrationTest.Fail(error.Message);
        }, searchedRoomSuccess =>
        {
            if (searchedRoom.Title == room.Title)
            {
                TearDown(room);
            }
        }));
    }

    void TearDown(Room room)
    {
        StartCoroutine(room.Delete(error =>
        {
            IntegrationTest.Fail("Failed to cleanup test room: " + error.Message);
        }, success =>
        {
            IntegrationTest.Pass();
        }));
    }
}