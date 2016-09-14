using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class Membership {
		public string Id { get; private set;}
		public string RoomId { get; set;}
		public string PersonId { get; set;}
		public string PersonDisplayName { get; set;}
		public string PersonEmail { get; set;}
		public bool IsModerator { get; set;}
		public bool IsMonitor { get; set;}
		public string Created { get; set;}

		/// <summary>
		/// Initializes a new <see cref="Cisco.Spark.Membership"/> object from an existing
		/// Spark membership.
		/// </summary>
		/// <param name="json">Json.</param>
		public Membership(string json) {
			Dictionary<string, object> membershipData = Json.Deserialize (json) as Dictionary<string, object>;
			Id = membershipData ["id"] as string;
			RoomId = membershipData ["roomId"] as string;
			PersonId = membershipData ["personId"] as string;
			PersonEmail = membershipData ["personEmail"] as string;
			PersonDisplayName = membershipData ["personDisplayName"] as string;
			IsModerator = (bool) membershipData ["isModerator"];
			IsMonitor = (bool) membershipData ["isMonitor"];
			Created = membershipData ["created"] as string;
		}

		/// <summary>
		/// Initializes a new <see cref="Cisco.Spark.Membership"/> object locally.
		/// </summary>
		public Membership(string roomId, string personId, string personEmail, bool isModerator) {
			RoomId = roomId;
			PersonId = personId;
			PersonEmail = personEmail;
			IsModerator = isModerator;
		}

		/// <summary>
		/// Initializes a new <see cref="Cisco.Spark.Membership"/> object locally.
		/// </summary>
		public Membership() { }

		/// <summary>
		/// Commits the current state of the local <see cref="Cisco.Spark.Membership"/> object to Spark.
		/// This will create a new <see cref="Cisco.Spark.Membership"/> if it doesn't exist. 
		/// </summary>
		/// <param name="callback">The created/updated <see cref="Cisco.Spark.Membership"/> from Spark</param>
		public IEnumerator Commit(Action<Membership> callback) {
			// Setup request from current state of Room object
			Request manager = GameObject.FindObjectOfType<Request> ();

			// Membership Data
			Dictionary<string, string> data = new Dictionary<string, string> ();

			// Create or Update?
			string resource;
			string httpVerb;
			if (Id == null) {
				// Creating a new Membership
				data ["roomId"] = RoomId;
				data ["personId"] = PersonId;
				data ["personEmail"] = PersonEmail;
				data ["isModerator"] = IsModerator.ToString ();
				resource = "memberships";
				httpVerb = UnityWebRequest.kHttpVerbPOST;
			} else {
				// Updating an existing Membership
				// Only changing <see cref="Cisco.Spark.Membership.IsModerator"/> is currently supported. 
				data ["isModerator"] = IsModerator.ToString ();
				resource = "memberships/" + Id;
				httpVerb = UnityWebRequest.kHttpVerbPUT;
			}

			// Make request
			using (UnityWebRequest www = manager.Generate(resource, httpVerb)) {
				byte[] raw_data = System.Text.Encoding.UTF8.GetBytes (Json.Serialize (data));
				www.uploadHandler = new UploadHandlerRaw (raw_data);
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError("Failed to Create Membership: " + www.error);
				} else {
					Membership membership = new Membership (www.downloadHandler.text);
					callback (membership);
				}
			}
		}

		/// <summary>
		/// Delete the specified <see cref="Cisco.Spark.Membership"/>
		/// </summary>
		public IEnumerator Delete() {
			if (Id != null) {
				Request manager = GameObject.FindObjectOfType<Request> ();
				using (UnityWebRequest www = manager.Generate ("memberships/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						Debug.LogError ("Failed to Delete Membership: " + www.error);
					}
				}
			}
		}
			
		/// <summary>
		/// Lists the memberships.
		/// </summary>
		/// <returns>The memberships.</returns>
		/// <param name="callback">Callback.</param>
		/// <param name="roomId">Room identifier.</param>
		/// <param name="personId">Person identifier.</param>
		/// <param name="personEmail">Person email.</param>
		/// <param name="max">Max.</param>
		/// TODO: Add optional memberships
		public static IEnumerator ListMemberships(Action<List<Membership>> callback, string roomId = null, string personId = null, string personEmail = null, int max = 0) {
			Request manager = GameObject.FindObjectOfType<Request> ();

			// Optional Parameters
			Dictionary<string, string> data = new Dictionary<string, string> ();
			if (roomId != null) {
				data ["roomId"] = roomId;
			} 
			if (personId != null) {
				data ["personId"] = roomId;
			}
			if (personEmail != null) {
				data ["personEmail"] = roomId;
			}
			if (max != 0) {
				data ["max"] = max.ToString ();
			}

			// Optional Parameters to URL query
			string queryString = System.Text.Encoding.UTF8.GetString (UnityWebRequest.SerializeSimpleForm (data));

			// Make Request
			using (UnityWebRequest www = manager.Generate ("memberships?" + queryString, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to List Memberships");
				} else {
					// Convert to Membership objects
					List<Membership> memberships = new List<Membership> ();
					Dictionary<string, object> json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					List<object> items = json ["items"] as List<object>;
					foreach (Dictionary<string, object> membership_json in items) {
						string reJsoned = Json.Serialize (membership_json);
						memberships.Add(new Membership (reJsoned));
					}
					callback (memberships);
				}
			}
		}

		/// <summary>
		/// Gets the membership details.
		/// </summary>
		/// <returns>The membership details.</returns>
		/// <param name="callback">The Membership.</param>
		/// <param name="membershipId">Membership identifier.</param>
		public static IEnumerator GetMembershipDetails(Action<Membership> callback, string membershipId) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("memberships/" + membershipId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to retrieve Membership");
				} else {
					callback(new Membership (www.downloadHandler.text));
				}
			}
		}
	}
}
