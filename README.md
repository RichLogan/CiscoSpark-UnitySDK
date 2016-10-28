# CiscoSpark-UnitySDK
Native Cisco Spark SDK for Unity 5.4+

You can follow the progress + development of this here: https://trello.com/b/BvpwAZYd/unity-spark-sdk

Basic `TODO` list:
- Better Testing
- Inheritance rewrite
- Documentation


## About
This library takes advantage of the new `UnityEngine.Networking.UnityWebRequest` functionality introduced in Unity 5.4, as well as running all methods via Coroutines and returning results and errors using `System.Action`, in order to be non-blocking. I took inspiration from the NodeJS callback style using error, response.

The SDK gives the following for each object:

- Ability to create, commit and delete
    - Committing a `Id=null` local object will create it on Spark
    - If an ID is set it will update
- Ability to List all objects from Spark matching a given query.
- Ability to retrieve a specific instance from Spark.
- A `SparkMessage` will be returned via the `error` callback if Spark cannot complete a request.

Please see the Spark Developer documentation at http://developer.ciscospark.com for details about specific requests. This SDK will always aim for parity with the APIs documented there.

## Setup
1. Import the scripts or UnityPackage.
2. Place the `Request` script on any GameObject.
3. Set the `AuthenticationString` variable in the Inspector.
    - `BaseUrl` defaults to: https://api.ciscospark.com/v1
    - Your `AuthenticationToken` or your bot's token can be found at http://developer.ciscospark.com

## Examples
Here is an example of sending a message to a given room from Unity (without knowing the `RoomId` beforehand):

```c#
using UnityEngine;
using Cisco.Spark;

public class Spark : MonoBehaviour {
	void Start() {
		StartCoroutine (Room.ListRooms (error => {
			if (error != null) {
				Debug.LogError(error.Message);
				foreach (var sparkError in error.Errors) {
					Debug.LogError(sparkError.Description);
				}
			}
		}, rooms => {
			foreach (var room in rooms) {
				if (room.Title == "Test Room") {
					var testMessage = new Message ();
					testMessage.RoomId = room.Id;
					testMessage.Markdown = "This message came from **unity**";
					StartCoroutine (testMessage.Commit (callback => Debug.Log ("Created message: " + testMessage.Id)));
				}
			}
		}));
	}
}
```

Here is an example of downloading all files from a given room, and placing them onto cubes:

```c#
using UnityEngine;
using Cisco.Spark;

public class Spark : MonoBehaviour {
	void Start() {
		StartCoroutine (Message.ListMessages ("Y2lzY29zcGFyazovL3VzL1JPT00vMzFhOTVkYTAtZjgwYi0xMWU1LWIyMjgtNTk1Mjc3YjMwNDli",
			error => {
				if (error != null) {
					Debug.LogError ("Failed: " + error.Message);
				}
			},
			messages => {
				foreach (var message in messages) {
					if (message.Files != null) {
						foreach (var file in message.Files) {
							StartCoroutine(file.Download (callback => {
								if (file.ReturnType == typeof(Texture2D)) {
									var test = GameObject.CreatePrimitive (PrimitiveType.Cube);
									test.name = file.Filename;
									test.transform.position = new Vector3(Random.Range (0, 25), Random.Range (0, 25), Random.Range (0, 25));
									test.GetComponent<Renderer>().material.mainTexture = callback as Texture2D;
								}
							}));
						}
					}
				}
			}
		));
	}
}
```

## Tests
Unfortunately, there is a lack of support for running Asynchronous operations in Unity Tests. Instead, there are `MonoBehaviour` test scripts included that will run chains of requests. To run them, just attach any of the `Test*` scripts and `Request` to a `GameObject`, and the results will be outputted to the console.

**Note: This will create/edit/destroy real test rooms/memberships/etc on the given Spark account, but they will clean up after themselves if possible.**
