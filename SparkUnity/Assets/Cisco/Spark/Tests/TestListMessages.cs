using UnityEngine;
using Cisco.Spark;

public class TestListMessages : MonoBehaviour
{
    Room testRoom;
    Message testMessage;

    void Start()
    {
        SetUp();
    }

    void SetUp()
    {
        // We need a test room for the messages.
        testRoom = new Room("Unity SDK Test Room", null);
        StartCoroutine(testRoom.Commit(error =>
        {
            IntegrationTest.Fail("Failed to create test room" + error.Message);
        }, success =>
        {
            // Need a message to look for.
            testMessage = new Message(testRoom);
            testMessage.Text = "Test Message";
            StartCoroutine(testMessage.Commit(error =>
            {
                TearDown();
                IntegrationTest.Fail("Failed to create test message: " + error.Message);
            }, createMessage =>
            {
                Test();
            }));
        }));
    }

    void Test()
    {
		// Find the created message.
        StartCoroutine(testRoom.ListMessages(error =>
        {
			TearDown();
			IntegrationTest.Fail(error.Message);
        }, results =>
        {
			IntegrationTest.Assert(results[0].Text.Equals("Test Message"));
			TearDown();
			IntegrationTest.Pass();
		}));
    }

    void TearDown()
    {
		// Cleanup the Room.
		StartCoroutine(testRoom.Delete(error => {
			IntegrationTest.Fail("Failed to cleanup test room: " + error.Message);
		}, success => {}));
    }
}