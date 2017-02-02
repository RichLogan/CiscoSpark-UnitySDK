using UnityEngine;
using Cisco.Spark;

public class TestDeleteMessage : MonoBehaviour
{
    Message testMessage;
    Room testRoom;

    void Start()
    {
        SetUp();
    }

    void SetUp()
    {
        // Need a test room for the message.
        testRoom = new Room("Unity SDK Test Room", null);
        StartCoroutine(testRoom.Commit(error =>
        {
			TearDown();
            IntegrationTest.Fail("Failed to make test Room: " + error.Message);
        }, success =>
        {
            // Now we need a test message to delete.
            testMessage = new Message(testRoom);
			testMessage.Text = "Test Message";
            StartCoroutine(testMessage.Commit(error =>
            {
				TearDown();
                IntegrationTest.Fail("Failed to create test message: " + error.Message);
            }, createdMessage =>
            {
                Test();
            }));
        }));
    }

    void Test()
    {
        // Delete the message.
        StartCoroutine(testMessage.Delete(error =>
        {
			TearDown();
            IntegrationTest.Fail(error.Message);
        }, success =>
        {
            IntegrationTest.Assert(success);
            TearDown();
			IntegrationTest.Pass();
        }));
    }

    void TearDown()
    {
        // Delete test room.
        StartCoroutine(testRoom.Delete(error =>
        {
            IntegrationTest.Fail("Failed to cleanup test Room: " + error.Message);
        }, success => { }));
    }
}