# CiscoSpark-UnitySDK
Native Cisco Spark SDK for Unity 5.4

Very much **unoffical**, **pre-release** and under **active development**! Pull requests most welcome.

This library takes advantage of the new `UnityEngine.Networking.UnityWebRequest` functionality introduced in Unity 5.4, as well as running all methods via Coroutines and returning results via `Actions`, in order to be non-blocking.

#### Setup
1. Import the scripts or UnityPackage
2. Place the `Request` script on any GameObject.
3. Set the `BaseUrl` and `AuthenticationString` variables in the Inspector.
    - `BaseUrl` is currently: https://api.ciscospark.com/v1
    - Your `AuthenticationToken` or your bot's token can be found at http://developer.ciscospark.com


#### Example
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
