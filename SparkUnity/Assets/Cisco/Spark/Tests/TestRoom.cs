using UnityEngine;
using Cisco.Spark;

/// <summary>
/// Class to test the <see cref="Cisco.Spark.Room"/> functionality. 
/// </summary>
public class TestRoom : MonoBehaviour {

	void Start () {
		int errorCount = 0;
		Debug.Log ("Running Room tests");

		// List Rooms
		StartCoroutine (Room.ListRooms (listRoomsError => {
			Debug.LogError("Couldn't list Rooms: " + listRoomsError.Message);
			errorCount++;
		}, rooms => {
			Debug.Log("List Rooms Passed!");
			int startRoomCount = rooms.Count;

			// Create New Room
			var testRoom = new Room("Test Room (CiscoSpark-UnitySDK)", null);
			StartCoroutine(testRoom.Commit (commitRoomError => {
				Debug.LogError("Couldn't commit Room: " + commitRoomError.Message);
				errorCount++;
			}, room => {
				testRoom = room;
				if (testRoom.Title != "Test Room (CiscoSpark-UnitySDK)") {
					Debug.LogError("Create Room Failed!");
					errorCount++;
				} else {
					Debug.Log("Create Room Passed!");
				}

				// Edit Room with Updated Title
				testRoom.Title = "Updated Test Room (CiscoSpark-UnitySDK)";
				StartCoroutine (testRoom.Commit(updateRoomError => {
					Debug.LogError("Couldn't update Room: " + updateRoomError.Message);
					errorCount++;
				}, updatedRoom => {
					testRoom = updatedRoom;
					if (testRoom.Title != "Updated Test Room (CiscoSpark-UnitySDK)") {
						Debug.LogError ("Update Room Failed!");
						errorCount++;
					} else {
						Debug.Log("Update Room Passed!");
					}

					// Get Room Details
					StartCoroutine (Room.GetRoomDetails (testRoom.Id, roomDetailsError => {
						Debug.LogError("Couldn't get Room details: " + roomDetailsError.Message);
						errorCount++;
					}, retrivedRoom => {
						testRoom = retrivedRoom;
						if (testRoom.Title != "Updated Test Room (CiscoSpark-UnitySDK)") {
							Debug.LogError ("GetRoomDetails Failed!");
						} else {
							Debug.Log("GetRoomDetails Passed!");
						}

						// Delete Room
						StartCoroutine (testRoom.Delete (deleteRoomError => {
							Debug.LogError("Couldn't get Room details: " + deleteRoomError.Message);
							errorCount++;
						}, result => StartCoroutine (Room.ListRooms (listRoomsError => {
							Debug.LogError ("Couldn't list Rooms: " + listRoomsError.Message);
							errorCount++;
						}, postDeleteRooms => {
							if (startRoomCount != postDeleteRooms.Count) {
								Debug.LogError ("Delete Room Failed!");
								errorCount++;
							} else {
								Debug.Log ("Delete Room Passed!");
							}
							// Finish and Report
							Debug.Log ("Finished Running Room Tests");
							if (errorCount == 0) {
								Debug.Log ("All tests passed!");
							} else {
								Debug.LogError (errorCount + " tests failed!");
							}
						}))));
					}));
				}));
			}));
		}));
	}
}
