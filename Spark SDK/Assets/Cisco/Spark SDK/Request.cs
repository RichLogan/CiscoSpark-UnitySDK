using UnityEngine;
using UnityEngine.Networking;

public class Request : MonoBehaviour {
	// Publically Set Variables
	public const string BaseUrl = "https://api.ciscospark.com/v1";
	public string AuthenticationToken = "";

	public UnityWebRequest Generate(string Resource, string RequestType) {
		// Setup Headers
		UnityWebRequest www = new UnityWebRequest (BaseUrl + "/" + Resource);
		www.SetRequestHeader ("Authorization", "Bearer " + AuthenticationToken);
		www.SetRequestHeader ("Content-type", "application/json; charset=utf-8");
		www.method = RequestType;
		www.downloadHandler = new DownloadHandlerBuffer();
		return www;
	}
}