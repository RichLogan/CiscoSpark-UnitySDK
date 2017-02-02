# Cisco Spark SDK for Unity [![Build Status](https://travis-ci.org/RichLogan/CiscoSpark-UnitySDK.svg?branch=master)](https://travis-ci.org/RichLogan/CiscoSpark-UnitySDK)
Native Cisco Spark SDK for Unity 5.4+

You can follow the progress + development of this here: https://trello.com/b/BvpwAZYd/unity-spark-sdk

Basic `TODO` list:
- Better Testing
- Cleanup
- Additional objects (Orgs, Licenses etc.)
- Pagination support

## Docs

Documentation can be found here: http://richlogan.co.uk/CiscoSpark-UnitySDK/ or [jump right into the code](http://richlogan.co.uk/CiscoSpark-UnitySDK/annotated.html).

## About
This library takes advantage of the new `UnityEngine.Networking.UnityWebRequest` functionality introduced in Unity 5.4. It talks to Spark through these web requests that run as coroutines to avoid blocking the main thread. Results are returned via `Action` callbacks.

This SDK will always aim for parity with the web APIs at http://developer.ciscospark.com. 

It provides:

- `Commit()` for creating or updating a record.
- `Delete()` for deletion.
- `Load()` for retrieve a record.
- `ListObject()` for querying records.

## Setup
1. Import the scripts or UnityPackage.
2. Place the `Cisco Spark Manager` prefab from Cisco/Spark/Prefabs into your scene.
3. Set the `AuthenticationString` variable of the `Request` component in the Inspector.
    - Your token (or your bot's token) can be found at http://developer.ciscospark.com
    - Support for integrations/OAuth is coming!

## Quickstart

The basic syntax/flow to run any of the requests is:

```c#
// Object is generated from ID or as a new object
var sparkObject = new Room/Membership/etc()
// Some operation is sent to Spark.
StartCoroutine(sparkObject.someFunction(requiredParams, error => {
    // This will run on an error.
    Debug.LogError("The operation failed: " + error.Message);
}, success => {
    // This will run on success.
    Debug.Log("The operation was a success!");
}, optionalParams));
```

- Any parameters required by Spark will come first.
- The `error` callback will return a `SparkMessage` if a request cannot be completed.
- The `success` callback will return `true` if an operation has succeded.
- For `ListObjects` success would be a list of the returned objects.
- Finally, any optional parameters would be given.

## Examples
Here is an example of sending a `Message` to a given `Room` from Unity (without knowing the `RoomId` beforehand).

```c#
using UnityEngine;
using Cisco.Spark;

public class Spark : MonoBehaviour {

	void SendMessageToTestRoom() {
        // List all rooms you are a member of.
        var listRooms = Room.ListRooms(error => {
            // Error callback would fire here.
            Debug.LogError(error.Message);
        }, rooms => {
            // Success! We now have all the rooms we're in.
            foreach (var room in rooms) {
                // Find the Test Room we want to post in.
                if (room.Title == "Test Room") {
                    // Create our message.
                    var testMessage = new Message(room);
                    testMessage.Markdown = "Hello from **Spark**";
                    
                    // Save our message to Spark!
                    var saveMessage = testMessage.Commit(messageError => {
                        Debug.LogError(messageError.Message);
                    }, success => {
                        Debug.Log("The message was saved to Spark successfully!");
                    });
                    StartCoroutine(saveMessage);
                }
            }
        });
        StartCoroutine(listRooms);
    }

    // When you know IDs, it's even easier!
    void SendToBob() {
        var bob = new Person(bobsId);
        var message = new Message(bob);
        message.Text = "Hi Bob!";
        StartCoroutine(message.Commit(error => {
            Debug.LogError("Couldn't send message to Bob :( " + error.Message);
        }, success => {
            Debug.Log("Sent to Bob :)");
        }));
    }
}
```

## Tests
Tests can be run by cloning from this repository (with `--recursive`) and opening the SparkIntegrationTests scene in Cisco/Spark/Tests. All tests can then be run using the Unity Integration Test Runner by selecting Integration Tests Runner from Unity Test Tools.

**Note: This will create/edit/destroy real test rooms/memberships/etc on the given Spark account, but they will clean up after themselves if possible.**
