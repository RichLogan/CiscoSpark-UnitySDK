using UnityEngine;

namespace Cisco.Spark {
	
	public class Spark : MonoBehaviour {

		Room createdRoom;
		int count;

		void Start() {
			// Create Room
//			Room room = new Room ("Unity SDK Testing", null);
//			StartCoroutine(room.Commit(newRoom => {
//				createdRoom = newRoom;
//			}));
			StartCoroutine (Room.ListRooms ("1234", 5, "hello"));
		}

//		void Update() {
//			count++;
//
//			// Change Name of Room
//			if (count == 300) {
//				createdRoom.Title = "Testing alter title";
//				StartCoroutine(createdRoom.Commit(output => {}));
//			}
//
//			// Delete Room
//			if (count == 400) {
//				StartCoroutine(createdRoom.Delete ());
//			}
//		}
	}
}