using UnityEngine;
using Cisco.Spark;

public class TestListRooms : MonoBehaviour {

	Room testRoom;

	void Start () {
		SetUp();
	}

	void SetUp() {
		// Create a test room to look for.
        var room = new Room("Unity SDK Test Room", null);
        StartCoroutine(room.Commit(error =>
        {
            // Error on Commit.
            IntegrationTest.Fail("Failed to create test room: " + error.Message);
        }, success =>
		{
			testRoom = room;
			Test();
		}));
	}

	void Test() {
		// List Rooms.
		StartCoroutine(Room.ListRooms(error => {
			IntegrationTest.Fail(error.Message);
		}, results => {
			var found = false;
			foreach (var room in results) {
				if (room.Title.Equals("Unity SDK Test Room")) {
					found = true;
				}
			}
			if (found) {
				TearDown();
			} else {
				IntegrationTest.Fail("Failed to find created room in list");
			}
		}));
	}

	void TearDown() {
		// Delete test room.
		StartCoroutine(testRoom.Delete(error => {
			IntegrationTest.Fail("Failed to cleanup test room: " + error.Message);
		}, success => {
			IntegrationTest.Pass();
		}));
	}
}
