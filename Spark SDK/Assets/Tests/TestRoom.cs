using UnityEngine;
using System.Threading;
using Cisco.Spark;

/// <summary>
/// Class to test the <see cref="Cisco.Spark.Room"/> functionality. 
/// </summary>
public class TestRoom : MonoBehaviour {

	void Start () {
		int errorCount = 0;
		Debug.Log ("Running Room tests");

		// List Rooms
		StartCoroutine (Room.ListRooms (rooms => {
			int startRoomCount = rooms.Count;

			// Create New Room
			Room testRoom = new Room("Test Room", null);
			StartCoroutine(testRoom.Commit (room => {
				testRoom = room;
				if (testRoom.Title != "Test Room") {
					Debug.LogError("Create Room Failed!");
					errorCount++;
				} else {
					Debug.Log("Create Room Passed!");
					startRoomCount++;
				}

				// Edit Room
				testRoom.Title = "Updated Test Room";
				StartCoroutine (testRoom.Commit(updatedRoom => {
					testRoom = updatedRoom;
					if (testRoom.Title != "Updated Test Room") {
						Debug.LogError ("Update Room Failed!");
						errorCount++;
					} else {
						Debug.Log("Update Room Passed!");
					}

					// Delete Room
					StartCoroutine (testRoom.Delete ());
					Thread.Sleep (1000); // Wait for Delete to finish
					startRoomCount--;
					StartCoroutine (Room.ListRooms (postDeleteRooms => {
						if (startRoomCount != postDeleteRooms.Count) {
							Debug.LogError("Delete Room Failed!");
							errorCount++;
						} else {
							Debug.Log("Delete Room Passed!");
						}


						// Finish and Report
						Debug.Log ("Finished Running Room Tests");
						if (errorCount == 0) {
							Debug.Log("All tests passed!");
						} else {
							Debug.LogError (errorCount + " tests failed!");
						}
					}));
				}));
			}));
		}));
	}
}
