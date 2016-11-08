using UnityEngine;
using Cisco.Spark;

public class TestSparkFile : MonoBehaviour {

	[Tooltip("A Room Id containing an uploaded OBJ file.")]
	public string RoomId = "";

	void Start () {
		StartCoroutine (Message.ListMessages (RoomId, error => Debug.LogError(error.Message), messages => {
			foreach (var message in messages) {
				if (message.Files != null) {
					foreach (var file in message.Files) {
						StartCoroutine (file.GetHeaders (callback => StartCoroutine (file.Download (downloadedObject => {
							if (file.ReturnType == typeof(GameObject)) {
								var go = (GameObject)downloadedObject;
								if (go.GetComponent<MeshFilter>().mesh != null) {
									Debug.Log("OBJ Download Successful");
								}
							}
						}))));
					}
				}
			}
		}));
	}
}
