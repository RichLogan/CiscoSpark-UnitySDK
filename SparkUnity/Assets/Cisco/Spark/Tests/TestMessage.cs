using UnityEngine;
using Cisco.Spark;

/// <summary>
/// Class to test the <see cref="Cisco.Spark.Message"/> functionality.
/// </summary>
public class TestMessage : MonoBehaviour {

	// Run all tests
	void Start () {
		int errorCount = 0;
		Debug.Log ("Running Message tests");

		// Create a room for tests
		var testRoom = new Room ("Test Room (CiscoSpark-UnitySDK", null);
		StartCoroutine (testRoom.Commit (error => {
			// Failed to create test room
			if (error != null) {
				errorCount++;
				Debug.LogError("Couldn't create target Room");
			}
		}, room => {
			// Test Room successful
			testRoom = room;
			if (testRoom.Id == null) {
				errorCount++;
				Debug.LogError ("Could not create target Room");
			}

			// Create a new Message
			var newMessage = new Message ();
			newMessage.RoomId = testRoom.Id;
			newMessage.Text = "Test message";

			// Commit Message
			StartCoroutine (newMessage.Commit (error => {
				if (error != null) {
					errorCount++;
					Debug.LogError("Couldn't commit Message: " + error.Message);
				}
			}, message => {
				newMessage = message;

				// List Messages From Room
				StartCoroutine (Message.ListMessages (testRoom.Id, error => {
					if (error != null) {
						errorCount++;
						Debug.LogError("Couldn't list Messages: " + error.Message);
					}
				}, messages => {
					// Check Message was successful
					if (messages [messages.Count - 1].Text != "Test message") {
						Debug.LogError ("Create Message Failed!");
						errorCount++;
					} else {
						Debug.Log ("Create Message Passed!");
					}

					// Delete the message
					StartCoroutine (newMessage.Delete (error => {
						if (error != null) {
							errorCount++;
							Debug.LogError("Couldn't delete message: " + error.Message);
						}
					}, result => StartCoroutine (Message.GetMessageDetails (newMessage.Id, error => {
						// This should error as the message has been deleted
						Debug.Log ("Successfully failed to get delete message details");
						// Clean up test Room
						StartCoroutine (testRoom.Delete (deleteError => {
							if (deleteError != null) {
								errorCount++;
								Debug.LogError ("Couldn't delete Test Room: " + deleteError.Message);
							}
						}, delete => {
							// Finish and Report
							Debug.Log ("Finished Running Message Tests");
							if (errorCount == 0) {
								Debug.Log ("All tests passed!");
							} else {
								Debug.LogError (errorCount + " tests failed!");
							}
						}));
					}, success => {
						if (success != null) {
							errorCount++;
							Debug.Log ("Shouldn't be able to get details for deleted message");
						}
					}))));
				})); 
			}));
		}));
	}
}
