using UnityEngine;
using Cisco.Spark;

public class TestCreateMessage : MonoBehaviour {

	// Use this for initialization
	void Start () {
		SetUp();
	}

	void SetUp() {
		// Need a test room for messages.
        var room = new Room("Unity SDK Test Room", null);
        StartCoroutine(room.Commit(error =>
        {
            // Error on Commit.
            IntegrationTest.Fail("Failed to create test room " + error.Message);
        }, success =>
		{
			Test(room);
		}));
	}

	void Test(Room room) {
		// Create and post the message.
		var message = new Message(room);
		message.Text = "Test Message";
		StartCoroutine(message.Commit(error => {
			IntegrationTest.Fail(error.Message);
		}, success => {
			TearDown(room);
		}));
	}

	void TearDown(Room room) {
		StartCoroutine(room.Delete(error => {
			IntegrationTest.Fail("Failed to cleanup test room" + error.Message);
		}, success => {
			IntegrationTest.Pass();
		}));
	}
}
