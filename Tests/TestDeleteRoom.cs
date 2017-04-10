using UnityEngine;
using Cisco.Spark;

public class TestDeleteRoom : MonoBehaviour
{
	void Start() {
		SetUp();
	}

	void SetUp() {
		// Create test room.
        var room = new Room("Unity SDK Test Room", null);
        StartCoroutine(room.Commit(error =>
        {
            // Error on Commit.
            IntegrationTest.Fail("Failed to create test room: " + error.Message);
        }, success =>
		{
			Test(room);
		}));
	}

	void Test(Room room) {
		StartCoroutine(room.Delete(error => {
			IntegrationTest.Fail(error.Message);
		}, success => {
			IntegrationTest.Pass();
		}));
	}
}