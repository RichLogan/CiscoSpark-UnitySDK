using UnityEngine;
using Cisco.Spark;

public class TestMessage : MonoBehaviour
{

    string testMessageContent = "Testing Create Message";
    Room testRoom = null;

    void Start()
    {
        CreateMessage();
    }

    void CreateMessage()
    {
        // Create Temporary Test Room.
        testRoom = new Room("Cisco Spark Unity Test Room", null);
        var roomCommit = testRoom.Commit(error =>
        {
            Debug.LogError("Couldn't create test room: " + error.Message);
        }, testRoomCreated =>
        {
            var testMessage = new Message(testRoom);
            testMessage.Text = testMessageContent;
            var messageCommit = testMessage.Commit(messageError =>
            {
                Debug.LogError("Create Message Failed: " + messageError.Message);
            }, createMessageSuccess =>
            {
                GetMessage(testMessage.Id);
            });
            StartCoroutine(messageCommit);
        });
        StartCoroutine(roomCommit);
    }

    void GetMessage(string id)
    {
        var existingMessage = new Message(id);
        var loadMessage = existingMessage.Load(error =>
        {
            Debug.LogError("Failed to load existing message: " + error.Message);
        }, success =>
        {
            if (existingMessage.Text == testMessageContent)
            {
                // Create is known to have passed here.
                Debug.Log("Create Message Passed!");
                // Get is known to have passed here.
                Debug.Log("Get Message Passed!");
                DeleteMessage(id);
            }
        });
        StartCoroutine(loadMessage);
    }

    void DeleteMessage(string id)
    {
        var message = new Message(id);
        var deleteMessage = message.Delete(error =>
        {
            Debug.LogError("Failed to delete message: " + error.Message);
        }, success =>
        {
            // Check it's been deleted.
            var checkDelete = new Message(message.Id);
            var getDeleted = checkDelete.Load(error =>
            {
                // This should fail.
                if (error.Message == "message not found")
                {
                    Debug.Log("Delete Message Passed!");
                    CleanUpRoom();
                }
                else
                {
                    Debug.LogError("Delete Message Failed: " + error.Message);
                }
            }, getDeleteSuccess =>
            {
                // This shouldn't succeed.
                Debug.LogError("Delete Message Failed");
            });
            StartCoroutine(getDeleted);
        });
        StartCoroutine(deleteMessage);
    }

    void CleanUpRoom()
    {
        StartCoroutine(testRoom.Delete(error =>
        {
            Debug.LogError("Failed to delete test room: " + error.Message);
        }, success =>
        {
            Debug.Log("Cleaned up test Room");
        }));
    }
}