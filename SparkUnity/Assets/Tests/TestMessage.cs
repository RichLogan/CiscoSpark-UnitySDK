using UnityEngine;
using System.Collections;
using System.Threading;
using Cisco.Spark;

/// <summary>
/// Class to test the <see cref="Cisco.Spark.Message"/> functionality.
/// </summary>
public class TestMessage : MonoBehaviour {

	// Run all tests
	void Start () {
		int errorCount = 0;
		Debug.Log ("Running Message tests");

		// Create a room for tests users
		var testRoom = new Room ("Test Room (CiscoSpark-UnitySDK", null);
		StartCoroutine (testRoom.Commit (room => {
			testRoom = room;
			if (testRoom.Id == null) {
				Debug.LogError ("Could not create target Room");
			}

			// Create a new Message
			var newMessage = new Message ();
			newMessage.RoomId = testRoom.Id;
			newMessage.Text = "Test message";
			StartCoroutine (newMessage.Commit (message => {
				newMessage = message;
				StartCoroutine (Message.ListMessages (testRoom.Id, messages => {
					if (messages [messages.Count - 1].Text != "Test message") {
						Debug.LogError ("Create Message Failed!");
						errorCount++;
					} else {
						Debug.Log ("Create Message Passed!");
					}

					// Delete the message
					StartCoroutine (newMessage.Delete ());
					Thread.Sleep (2000); // Wait for Delete to finish
					StartCoroutine (Message.GetMessageDetails (newMessage.Id, postDeleteMessage => {
						StartCoroutine (testRoom.Delete ());

						// Finish and Report
						Debug.Log ("Finished Running Message Tests");
						if (errorCount == 0) {
							Debug.Log ("All tests passed!");
						} else {
							Debug.LogError (errorCount + " tests failed!");
						}
					}));
				})); 
			}));
		}));
	}
}
