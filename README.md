# CiscoSpark-UnitySDK
Native Cisco Spark SDK for Unity 5.4+

You can follow the progress + development of this here: https://trello.com/b/BvpwAZYd/unity-spark-sdk

Basic `TODO` list:
- Better Testing
- Full Documentation

## About
This library takes advantage of the new `UnityEngine.Networking.UnityWebRequest` functionality introduced in Unity 5.4, as well as running all methods via Coroutines and returning results and errors using `System.Action`, in order to be non-blocking. I took inspiration from the NodeJS callback style using error, response.

The SDK gives the following for each object:

- CRUD operations for all SparkObjects.
    - Committing a `Id=null` local object will create it on Spark.
    - If an ID is set it will update the record on Spark.
- Ability to List all objects from Spark matching a given query.
- Ability to retrieve a specific instance from Spark.
- A `SparkMessage` will be returned via the `error` callback if Spark cannot complete a request.

Please see the Spark Developer documentation at http://developer.ciscospark.com for details about specific requests. This SDK will always aim for parity with the APIs documented there.

## Setup
1. Import the scripts or UnityPackage.
2. Place the `Cisco Spark Manager` prefab into your scene.
3. Set the `AuthenticationString` variable in the Inspector.
    - Your `AuthenticationToken` or your bot's token can be found at http://developer.ciscospark.com

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

Until proper documentation is generated you can have a look at the different operations as shown in the Tests.

## Tests
Unfortunately, there is a lack of support for running Asynchronous operations in the Unity Test Tools, and I still need to write proper unit tests for the non-async parts of the SDK.
Instead, there are `MonoBehaviour` test scripts included that will run chains of requests. To run them, just attach any of the `Test*` scripts to a GameObject, and the results will be outputted to the console.
Some of them require known and existing Person IDs to be added (as I can't generate Person objects without admin rights on a Spark instance.)

**Note: This will create/edit/destroy real test rooms/memberships/etc on the given Spark account, but they will clean up after themselves if possible.**
