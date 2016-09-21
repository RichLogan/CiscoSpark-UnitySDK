using UnityEngine;
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

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Person"/>
		/// class from an existing Spark record
		/// </summary>
		/// <param name="json">JSON from Cisco Spark</param>
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

		/// <summary>
		/// Downloads the user's avatar as a Texture
		/// </summary>
		/// <returns>Texture of an avatar</returns>
		/// <param name="callback">Callback.</param>
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

		/// <summary>
		/// Gets details of a Person from Spark
		/// </summary>
		/// <returns>The Person object</returns>
		/// <param name="personId">Person identifier.</param>
		/// <param name="callback">Callback.</param>
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

		/// <summary>
		/// Lists the people matched by the query
		/// </summary>
		/// <returns>List of People</returns>
		/// <param name="callback">Callback.</param>
		/// <param name="email">Email to search on</param>
		/// <param name="displayName">Display name to search on</param>
		/// <param name="max">Max number of people to return</param>
		public static IEnumerator ListPeople(Action<List<Person>> callback, string email = null, string displayName = null, int max = 0) {
			if (email == null && displayName == null) {
				Debug.LogError ("Email or displayName should be specified.");
			}

			Request manager = GameObject.FindObjectOfType<Request> ();

			// Optional Parameters
			var data = new Dictionary<string, string> ();
			if (email != null) {
				data ["email"] = email;
			}
			if (displayName != null) {
				data ["displayName"] = displayName;
			}
			if (max != 0) {
				data ["max"] = max.ToString ();
			}

			// Optional Parameters to URL query
			string queryString = System.Text.Encoding.UTF8.GetString (UnityWebRequest.SerializeSimpleForm (data));

			// Make Request
			using (UnityWebRequest www = manager.Generate ("people?" + queryString, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to List People");
				} else {
					// Convert to Person objects
					var people = new List<Person> ();
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					var items = json ["items"] as List<object>;
					foreach (Dictionary<string, object> person_json in items) {
						string reJsoned = Json.Serialize (person_json);
						people.Add(new Person (reJsoned));
					}
					callback (people);
				}
			}
		}
	}
}
