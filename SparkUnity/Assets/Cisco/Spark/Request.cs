﻿using UnityEngine;
using UnityEngine.Networking;

public class Request : MonoBehaviour {
	// Publically Set Variables
	public const string BaseUrl = "https://api.ciscospark.com/v1";
	public string AuthenticationToken = "";

	public UnityWebRequest Generate(string resource, string requestType) {
		// Setup Headers
		var www = new UnityWebRequest (BaseUrl + "/" + resource);
		www.SetRequestHeader ("Authorization", "Bearer " + AuthenticationToken);
		www.SetRequestHeader ("Content-type", "application/json; charset=utf-8");
		www.method = requestType;
		www.downloadHandler = new DownloadHandlerBuffer();
		return www;
	}
}