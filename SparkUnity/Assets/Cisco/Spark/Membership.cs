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
		public DateTime Created { get; private set;}

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Membership"/> class.
		/// </summary>
		public Membership() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Membership"/> class.
		/// </summary>
		/// <param name="roomId">Room identifier.</param>
		/// <param name="personId">Person identifier.</param>
		/// <param name="personEmail">Person email.</param>
		/// <param name="isModerator">If set to <c>true</c> is moderator.</param>
		public Membership(string roomId, string personId, string personEmail, bool isModerator) {
			RoomId = roomId;
			PersonId = personId;
			PersonEmail = personEmail;
			IsModerator = isModerator;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Membership"/> class.
		/// </summary>
		/// <param name="membershipData">Membership data.</param>
		Membership(Dictionary<string, object> membershipData) {
			try {
				Id = membershipData ["id"] as string;
				RoomId = membershipData ["roomId"] as string;
				PersonId = membershipData ["personId"] as string;
				PersonEmail = membershipData ["personEmail"] as string;
				PersonDisplayName = membershipData ["personDisplayName"] as string;
				IsModerator = (bool) membershipData ["isModerator"];
				IsMonitor = (bool) membershipData ["isMonitor"];
				Created = DateTime.Parse ((string) membershipData ["created"]);
			} catch (KeyNotFoundException) {
				Debug.Log ("Couldn't parse Membership");
			}
		}

		/// <summary>
		/// Commit the specified error and result.
		/// </summary>
		/// <param name="error">Error.</param>
		/// <param name="result">Result.</param>
		public IEnumerator Commit(Action<SparkMessage> error, Action<Membership> result) {
			// Setup request from current state of Room object
			var manager = GameObject.FindObjectOfType<Request> ();

			// Membership Data
			var data = new Dictionary<string, string> ();

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
					// Parse Response
					var membershipData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (membershipData.ContainsKey ("message")) {
						// Spark Error
						error (new SparkMessage (membershipData));
						result (null);
					} else {
						// Create local Membership object
						error(null);
						result(new Membership (membershipData));
					}
				}
			}
		}

		/// <summary>
		/// Delete the specified error and result.
		/// </summary>
		/// <param name="error">Error.</param>
		/// <param name="result">Result.</param>
		public IEnumerator Delete(Action<SparkMessage> error, Action<bool> result) {
			if (Id != null) {
				var manager = GameObject.FindObjectOfType<Request> ();
				using (UnityWebRequest www = manager.Generate ("memberships/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						// Network Error
						Debug.LogError ("Failed to Delete Membership: " + www.error);
					} else {
						// Delete returns 204 on success
						if (www.responseCode == 204) {
							error (null);
							result (true);
						} else {
							// Delete Failed
							var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
							error (new SparkMessage (json));
							result (false);
						}
					}
				}
			}
		}
			
		/// <summary>
		/// Lists the memberships.
		/// </summary>
		/// <returns>The memberships.</returns>
		/// <param name="error">Error.</param>
		/// <param name="result">Result.</param>
		/// <param name="roomId">Room identifier.</param>
		/// <param name="personId">Person identifier.</param>
		/// <param name="personEmail">Person email.</param>
		/// <param name="max">Max.</param>
		public static IEnumerator ListMemberships(Action<SparkMessage> error, Action<List<Membership>> result, string roomId = null, string personId = null, string personEmail = null, int max = 0) {
			var manager = GameObject.FindObjectOfType<Request> ();

			// Optional Parameters
			var data = new Dictionary<string, string> ();
			if (roomId != null) {
				data ["roomId"] = roomId;
			} 
			if (personId != null) {
				data ["personId"] = personId;
			}
			if (personEmail != null) {
				data ["personEmail"] = personEmail;
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
					// Network error
					Debug.LogError("Failed to List Memberships: " + www.error);
				} else {
					// Request succeeded, parse response
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;

					// Check for Spark side errors
					if (json.ContainsKey ("message")) {
						error (new SparkMessage (json));
						result (null);
					} else {
						// Convert to Membership objects
						var memberships = new List<Membership> ();
						var items = json ["items"] as List<object>;
						foreach (var membership in items) {
							memberships.Add (new Membership (membership as Dictionary<string, object>));
						}
						result (memberships);
						error (null);
					}
				}
			}
		}
			
		/// <summary>
		/// Gets the membership details.
		/// </summary>
		/// <returns>The membership details.</returns>
		/// <param name="error">Error.</param>
		/// <param name="result">Result.</param>
		/// <param name="membershipId">Membership identifier.</param>
		public static IEnumerator GetMembershipDetails(string membershipId, Action<SparkMessage> error, Action<Membership> result) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("memberships/" + membershipId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();

				if (www.isError) {
					// Network error
					Debug.LogError (www.error);
				} else {
					// Parse Response
					var membershipData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (membershipData.ContainsKey ("message")) {
						// Error Callback
						error (new SparkMessage (membershipData));
						result(null);
					} else {
						// Result callback
						error (null);
						result(new Membership (membershipData));
					}
				}
			}
		}
	}
}
