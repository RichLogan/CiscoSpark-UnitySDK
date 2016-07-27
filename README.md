# CiscoSpark-UnitySDK
Native CiscoSpark SDK for Unity

```c#
StartCoroutine (Room.ListRooms ("<TEAM_ID>", 5, "group", rooms => {
    Debug.Log ("Found: " + rooms.Count + " rooms");
	foreach (Room room in rooms) {
		Debug.Log(room.Title);
	}
}));
```
