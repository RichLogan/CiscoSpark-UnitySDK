# Cisco Spark SDK for Unity [![Build Status](https://travis-ci.org/RichLogan/CiscoSpark-UnitySDK.svg?branch=master)](https://travis-ci.org/RichLogan/CiscoSpark-UnitySDK)

Native Cisco Spark SDK for Unity 5.4+

An example starter project can be found here: https://github.com/RichLogan/CiscoSpark-UnityProject

## Docs

Documentation can be found here: http://richlogan.co.uk/CiscoSpark-UnitySDK/.

## About
This library takes advantage of the new `UnityEngine.Networking.UnityWebRequest` functionality introduced in Unity 5.4. It talks to Spark through these web requests that run as coroutines to avoid blocking the main thread. Results are returned via `Action` callbacks.

This SDK will always aim for parity with the web APIs at http://developer.ciscospark.com.

It provides:

- `Commit()` for creating or updating a record.
- `Delete()` for deletion.
- `Load()` for populating an object with a Spark record.
- Implementations of `ListObject()` for querying records.

## Setup
1. Either:
	- Import the UnityPackage from releases into your Unity project.
	- OR: Clone this repository (or submodule) into your project's `Assets/` folder.
	- OR: Clone the example project located here https://github.com/RichLogan/CiscoSpark-UnityProject
2. You will need the `MiniJSON.cs` script if you're not already using it. I don't include it as it will cause conflicts if you already use it. You can get it here: https://github.com/Jackyjjc/MiniJSON.cs/blob/master/MiniJSON.cs
3. Place the `Request` and `SparkResources` scripts into your scene.
4. Set the `AuthenticationString` variable of the `Request` component in the Inspector.
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
        var bob = Person.FromId(bobsId);
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

One other thing to note is how the SDK creates objects as a result of most queries:
  - Spark usually only returns IDs and metadata for any calls that are not a specific "GetDetails" call.
  - For objects created by these "indirect" requests, you will need to call `Load()` on them.
  - In the below example, listing messages in a Room will not populate the resultant message objects with the actual content (Text or Files) until you call `Load()`.
  - Likewise, a Person object would be created for `message.Author`, but again you will have to call `Load()` on it to retrieve anything other than ID.

```c#
StartCoroutine(someRoom.ListMessages(error => {}, results => {
    // Prints null.
    Debug.Log(results[0].Text);
    foreach (var message in results) {
        StartCoroutine(message.Load(error => {}, success => {
            // Prints the message.
            Debug.Log(message.Text);
        }));
    }
}));
```

## Tests
Tests can be run and opening the SparkIntegrationTests scene in Spark/Tests and setting your auth token in Request. All tests can then be run using the Unity Integration Test Runner by selecting Integration Tests Runner from the [Unity Test Tools](https://www.assetstore.unity3d.com/en/#!/content/13802)

**Note: This will create/edit/destroy real test rooms/memberships/etc on the given Spark account, but they will clean up after themselves when possible.**
