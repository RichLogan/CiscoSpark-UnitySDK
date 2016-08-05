# CiscoSpark-UnitySDK
Native Cisco Spark SDK for Unity 5.4

Very much **unoffical**, **pre-release** and under **active development**! Pull requests most welcome.

This library takes advantage of the new `UnityEngine.Networking.UnityWebRequest` functionality introduced in Unity 5.4, as well as running all methods via Coroutines and returning results via `Actions`, in order to be non-blocking.

## Setup
1. Import the scripts or UnityPackage
2. Place the `Request` script on any GameObject.
3. Set the `BaseUrl` and `AuthenticationString` variables in the Inspector.
    - `BaseUrl` is currently: https://api.ciscospark.com/v1
    - Your `AuthenticationToken` or your bot's token can be found at http://developer.ciscospark.com


## Examples
Here is an example of sending a message to a given room from Unity:

```c#
using UnityEngine;
using Cisco.Spark;

public class Spark : MonoBehaviour {
	void Start() {
		StartCoroutine (Room.ListRooms (rooms => {
			foreach (Room room in rooms) {
				if (room.Title == "Unity Test Room") {
					Message testMessage = new Message();
					testMessage.RoomId = room.Id;
					testMessage.Markdown = "This message came from **unity**";
					StartCoroutine (testMessage.Commit(message => {
						Debug.Log("Created message: " + message.Id);
					}));
				}
			}
		}));
	}
}
```

Here is an example of downloading all files from a given room, and placing them onto cubes:

```c#
StartCoroutine (Message.ListMessages (<ROOM_ID>, messages => {
    foreach (Message message in messages) {
        if (message.Files != null) {
            foreach (SparkFile file in message.Files) {
                StartCoroutine(file.Download (callback => {
                    if (file.returnType == typeof(UnityEngine.Texture2D)) {
                        GameObject test = GameObject.CreatePrimitive (PrimitiveType.Cube);
                        test.GetComponent<Renderer>().material.mainTexture = callback as Texture2D;
                    }
                }));
			}
		}
	}
}));
```

## Tests
Unfortunately, it is not possible to run tests for the SDK using the builtin Unity Test Tools, due to a lack of support for running Asynchronous operations. As a result, I have created some `MonoBehaviour` scripts that will run the tests that can be found in `Assets/Tests` in order to simulate the environment they will be run in. To run them, just attach any of the `Test*` scripts and `Request` to a `GameObject`, and the results will be outputted to the console.

**Note: This will create/edit/destroy real test rooms/memberships/etc on the given Spark account, but they will clean up after themselves if possible.**
