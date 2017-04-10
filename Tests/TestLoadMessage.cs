using UnityEngine;
using Cisco.Spark;

public class TestLoadMessage : MonoBehaviour {

	// Use this for initialization
	void Start () {
		SetUp();
	}

	void SetUp() {
		// Need a test room.
		var room = new Room("Unity SDK Test Room", null);
		StartCoroutine(room.Commit(error => {
			IntegrationTest.Fail("Failed to create test room: " + error.Message);
		}, success => {
			// Need a test message.
			var testMessage = new Message(room);
			testMessage.Text = "Test Message";
			StartCoroutine(testMessage.Commit(messageError => {
				IntegrationTest.Fail("Failed to create Test Message: " + messageError.Message);
			}, messageSuccess => {
				Test(testMessage);
			}));
		}));
	}
	
	void Test(Message message) {
		var loadMessage = new Message(message.Id);
		StartCoroutine(loadMessage.Load(error => {
			IntegrationTest.Fail(error.Message);
		}, success => {
			IntegrationTest.Assert(loadMessage.Text == message.Text);
			TearDown(message);
		}));
	}

	void TearDown(Message message) {
		// Cleanup Test Room.
		StartCoroutine(message.Room.Delete(error => {
			IntegrationTest.Fail("Failed to cleanup test room: " + error.Message);
		}, success => {
			IntegrationTest.Pass();
		}));
	}
}
