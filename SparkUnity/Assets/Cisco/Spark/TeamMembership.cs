using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class TeamMembership {
		public string Id { get; private set; }
		public string TeamId { get; set; }
		public string PersonId { get; set; }
		public string PersonEmail { get; set; }
		public string PersonDisplayName { get; set; }
		public bool IsModerator { get; set; }
		public DateTime Created { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.TeamMembership"/> class.
		/// </summary>
		public TeamMembership() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.TeamMembership"/> class.
		/// </summary>
		/// <param name="teamId">Team identifier.</param>
		/// <param name="personId">Person identifier.</param>
		/// <param name="personEmail">Person email.</param>
		/// <param name="isModerator">If set to <c>true</c> is moderator.</param>
		public TeamMembership(string teamId, string personId=null, string personEmail=null, bool isModerator=false) {
			// Argument checking
			if (personId == null && personEmail == null) {
				throw new ArgumentNullException ("personId","One of PersonId and PersonEmail must be given");
			}
			TeamId = teamId;
			PersonId = personId;
			PersonEmail = personEmail;
			IsModerator = isModerator;
		}

		/// <summary>
		/// Initializes a new instance of <see cref="Cisco.Spark.TeamMembership"/> from Spark.
		/// </summary>
		/// <param name="teamMembershipData">Team Membership data.</param>
		TeamMembership(Dictionary<string, object> teamMembershipData) {
			try {
				Id = teamMembershipData ["id"] as string;
				TeamId = teamMembershipData ["teamId"] as string;
				PersonId = teamMembershipData ["personId"] as string;
				PersonEmail = teamMembershipData ["personEmail"] as string;
				PersonDisplayName = teamMembershipData ["personDisplayName"] as string;
				IsModerator = (bool) teamMembershipData ["isModerator"];
				Created = DateTime.Parse ((string) teamMembershipData ["created"]);
			} catch (KeyNotFoundException) {
				Debug.Log ("Couldn't parse Team Membership");
			}
		}

		/// <summary>
		/// Commit the specified error and result.
		/// </summary>
		/// <param name="error">Error.</param>
		/// <param name="result">Result.</param>
		public IEnumerator Commit(Action<SparkMessage> error, Action<TeamMembership> result) {
			var manager = GameObject.FindObjectOfType<Request> ();

			// Membership Data
			var data = new Dictionary<string, string> ();

			// Create or Update?
			string resource;
			string httpVerb;
			if (Id == null) {
				// Creating a new Membership
				data ["teamId"] = TeamId;
				data ["personId"] = PersonId;
				data ["personEmail"] = PersonEmail;
				data ["isModerator"] = IsModerator.ToString ();
				resource = "team/memberships";
				httpVerb = UnityWebRequest.kHttpVerbPOST;
			} else {
				// Updating an existing Membership
				// Only changing <see cref="Cisco.Spark.Membership.IsModerator"/> is currently supported. 
				data ["isModerator"] = IsModerator.ToString ();
				resource = "team/memberships/" + Id;
				httpVerb = UnityWebRequest.kHttpVerbPUT;
			}

			// Make request
			using (UnityWebRequest www = manager.Generate(resource, httpVerb)) {
				byte[] raw_data = System.Text.Encoding.UTF8.GetBytes (Json.Serialize (data));
				www.uploadHandler = new UploadHandlerRaw (raw_data);
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError("Failed to Create Team Membership: " + www.error);
				} else {
					// Parse Response
					var teamMembershipData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (teamMembershipData.ContainsKey ("message")) {
						// Spark Error
						error (new SparkMessage (teamMembershipData));
						result (null);
					} else {
						// Create local TeamMembership object
						error(null);
						result(new TeamMembership (teamMembershipData));
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
				using (UnityWebRequest www = manager.Generate ("team/memberships/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						// Network Error
						Debug.LogError ("Failed to Delete Team Membership: " + www.error);
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
		/// <param name="teamId">Team identifier.</param>
		/// <param name="personId">Person identifier.</param>
		/// <param name="personEmail">Person email.</param>
		/// <param name="max">Max.</param>
		public static IEnumerator ListTeamMemberships(Action<SparkMessage> error, Action<List<TeamMembership>> result, string teamId = null, string personId = null, string personEmail = null, int max = 0) {
			var manager = GameObject.FindObjectOfType<Request> ();

			// Optional Parameters
			var data = new Dictionary<string, string> ();
			if (teamId != null) {
				data ["teamId"] = teamId;
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
			using (UnityWebRequest www = manager.Generate ("team/memberships?" + queryString, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();

				if (www.isError) {
					// Network error
					Debug.LogError("Failed to List Team Memberships: " + www.error);
				} else {
					// Request succeeded, parse response
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;

					// Check for Spark side errors
					if (json.ContainsKey ("message")) {
						error (new SparkMessage (json));
						result (null);
					} else {
						// Convert to TeamMembership objects
						var teamMemberships = new List<TeamMembership> ();
						var items = json ["items"] as List<object>;
						foreach (var teamMembership in items) {
							teamMemberships.Add (new TeamMembership (teamMembership as Dictionary<string, object>));
						}
						result (teamMemberships);
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
		/// <param name="teamMembershipId">Membership identifier.</param>
		public static IEnumerator GetTeamMembershipDetails(string teamMembershipId, Action<SparkMessage> error, Action<TeamMembership> result) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("team/memberships/" + teamMembershipId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();

				if (www.isError) {
					// Network error
					Debug.LogError (www.error);
				} else {
					// Parse Response
					var teamMembershipData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (teamMembershipData.ContainsKey ("message")) {
						// Error Callback
						error (new SparkMessage (teamMembershipData));
						result(null);
					} else {
						// Result callback
						error (null);
						result(new TeamMembership (teamMembershipData));
					}
				}
			}
		}
	}
}
