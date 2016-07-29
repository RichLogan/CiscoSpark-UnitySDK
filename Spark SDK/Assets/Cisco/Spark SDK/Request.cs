using UnityEngine;
using UnityEngine.Networking;

public class Request : MonoBehaviour {
	// Publically Set Variables
	public string BaseUrl = "https://api.ciscospark.com/v1";
	public string AuthenticationString = "";

	public UnityWebRequest Generate(string Resource, string RequestType) {
		// Setup Headers
		UnityWebRequest www = new UnityWebRequest (BaseUrl + "/" + Resource);
		www.SetRequestHeader ("Authorization", "Bearer " + AuthenticationString);
		www.SetRequestHeader ("Content-type", "application/json; charset=utf-8");
		www.method = RequestType;
		www.downloadHandler = new DownloadHandlerBuffer();
		return www;
	}
}