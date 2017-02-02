using UnityEngine;
using Cisco.Spark;

public class TestUpdateRoom : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        SetUp();
    }

    void SetUp()
    {
		// Create test room.
		var room = new Room("Unity SDK Test Room", null);
        StartCoroutine(room.Commit(error =>
        {
            // Error on Commit.
            IntegrationTest.Fail("Failed to create test room" + error.Message);
        }, success => {
			Test(room);
		}));
    }

    void Test(Room room)
    {
		// Update the room.
		room.Title = "Unity SDK Test Room - Updated";
		StartCoroutine(room.Commit(error =>
        {
            // Error on Commit.
            IntegrationTest.Fail(error.Message);
        }, success => {
			TearDown(room);
		}));
    }

    void TearDown(Room room)
    {
		// Clean the test room.
		StartCoroutine(room.Delete(error => {
			IntegrationTest.Fail("Failed to cleanup test room: " + error.Message);
		}, success => {
			IntegrationTest.Pass();
		}));
    }
}
