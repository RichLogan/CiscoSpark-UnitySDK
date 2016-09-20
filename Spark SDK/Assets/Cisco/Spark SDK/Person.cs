﻿using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class Person {
		public string Id { get; private set;}
		public string DisplayName { get; set;}
		public string Avatar { get; set;}
		public string Created { get; private set;}
		public List<string> Emails { get; set;}
		public string Status { get; private set;}

		public Person(string json) {
			var details = Json.Deserialize (json) as Dictionary<string, object>;
			Id = (string) details ["id"];
			DisplayName = (string) details ["displayName"];
			Avatar = (string) details["avatar"];
			// TODO: Created as DateTime
			Created = (string) details ["created"];
			Status = (string) details ["status"];

			object emails;
			if (details.TryGetValue ("emails", out emails)) {
				Emails = new List<string> ();
				var listOfEmails = emails as List<object>;
				foreach (var email in listOfEmails) {
					Emails.Add (email as string);
				}
			}
		}

		public Person(List<string> emails, string displayName, string avatar) {
			Emails = emails;
			DisplayName = displayName;
			Avatar = avatar;
		}

		public IEnumerator DownloadAvatar(Action<Texture> callback) {
			UnityWebRequest www = UnityWebRequest.GetTexture (Avatar);
			yield return www.Send ();
			if (www.isError) {
				Debug.LogError ("Failed to Download Avatar: " + www.error);
			} else {
				Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
				callback (texture);
			}
		}

		public IEnumerator Commit() {
			// Setup request from current state of Person object
			var manager = GameObject.FindObjectOfType<Request> ();
			var data = new Dictionary<string, string> ();

			// Create or Update?
			string httpVerb;
			if (Id == null) {
				// Creating a new user
				httpVerb = UnityWebRequest.kHttpVerbPOST;
			} else {
				// Updating a previous user
				httpVerb = UnityWebRequest.kHttpVerbPUT;
			}

			// Make request
			using (UnityWebRequest www = manager.Generate("people", httpVerb)) {
				byte[] raw_data = System.Text.Encoding.UTF8.GetBytes (Json.Serialize (data));
				www.uploadHandler = new UploadHandlerRaw (raw_data);
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError("Failed to Create User: " + www.error);
				}
			}
		}

		public IEnumerator Delete() {
			if (Id != null) {
				var manager = GameObject.FindObjectOfType<Request> ();
				using (UnityWebRequest www = manager.Generate ("people/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						Debug.LogError ("Failed to Delete User: " + www.error);
					}
				}
			}
		}
		
		static public IEnumerator GetPersonDetails(string personId, Action<Person> callback) {
			var manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate("people/" + personId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.error == null) {
					var person = new Person (www.downloadHandler.text);
					callback (person);
				} else {
					Debug.LogError ("Failed to Get Person Details: " + www.error);
					callback (null);
				}
			}
		}
	}
}