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
		public string FirstName { get; set;}
		public string LastName { get; set;}
		public string Avatar { get; set;}
		public DateTime Created { get; private set;}
		public List<string> Emails { get; set;}
		public string Status { get; private set;}

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Person"/>
		/// class from an existing Spark record
		/// </summary>
		/// <param name="details">Person data from Cisco Spark</param>
		Person(Dictionary<string, object> details) {
			Id = (string) details ["id"];
			DisplayName = (string) details ["displayName"];
			FirstName = (string) details ["firstName"];
			LastName = (string) details ["lastName"];
			Avatar = (string) details["avatar"];
			Created = DateTime.Parse ((string) details ["created"]);

			// Status not always implemented
			object status;
			if (details.TryGetValue ("status", out status)) {
				Status = (string) details ["status"];
			}

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
		/// <param name="error">Error if any from Spark</param>
		/// <param name="callback">Callback.</param>
		public IEnumerator DownloadAvatar(Action<SparkMessage> error, Action<Texture> callback) {
			UnityWebRequest www = UnityWebRequest.GetTexture (Avatar);
			yield return www.Send ();
			if (www.isError) {
				Debug.LogError ("Failed to Download Avatar: " + www.error);
			} else {
				var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
				if (texture) {
					callback (texture);
					error (null);
				} else {
					// TODO: Check what happens on failed avatar calls
					Debug.LogError ("Download avatar failed. Check implementation.");
					callback (null);
					error (null);
				}
			}
		}

		/// <summary>
		/// Gets details of the currently authenticated user from Spark.
		/// </summary>
		/// <returns>The Person object.</returns>
		/// <param name="error">Error.</param>
		/// <param name="callback">Callback.</param>
		static public IEnumerator GetPersonDetails(Action<SparkMessage> error, Action<Person> callback) {
			yield return GetPersonDetails ("me", error, callback);
		}

		/// <summary>
		/// Gets details of a Person from Spark
		/// </summary>
		/// <returns>The Person object</returns>
		/// <param name="personId">Person identifier.</param>
		/// <param name="error">Error from Spark</param>
		/// <param name="result">Callback.</param>
		public static IEnumerator GetPersonDetails(string personId, Action<SparkMessage> error, Action<Person> result) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("people/" + personId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();

				if (www.isError) {
					// Network error
					Debug.LogError (www.error);
				} else {
					// Parse Response
					var personData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (personData.ContainsKey ("message")) {
						// Error Callback
						error (new SparkMessage (personData));
						result(null);
					} else {
						// Result callback
						error (null);
						result(new Person (personData));
					}
				}
			}
		}

		/// <summary>
		/// Lists the people matched by the query
		/// </summary>
		/// <returns>List of People</returns>
		/// <param name="error">Error from Spark.</param>
		/// <param name="result">Callback.</param>
		/// <param name="email">Email to search on.</param>
		/// <param name="displayName">Display name to search on.</param>
		/// <param name="max">Max number of people to return.</param>
		public static IEnumerator ListPeople(Action<SparkMessage> error, Action<List<Person>> result, string email = null, string displayName = null, int max = 0) {
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
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (json.ContainsKey ("message")) {
						error (new SparkMessage (json));
						result (null);
					} else {
						// Convert to Membership objects
						var people = new List<Person> ();
						var items = json ["items"] as List<object>;
						foreach (var person in items) {
							people.Add (new Person (person as Dictionary<string, object>));
						}
						result (people);
						error (null);
					}
				}
			}
		}
	}
}
