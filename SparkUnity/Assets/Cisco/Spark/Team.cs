using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class Team {
		public string Id { get; private set;}
		public string Name { get; set;}
		public DateTime Created { get; private set;}

		/// <summary>
		/// Initialize a new instance of the <see cref="Cisco.Spark.Team"/> class locally.
		/// </summary>
		/// <param name="name">The name of the team</param>
		public Team(string name) {
			Name = name;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Cisco.Spark.Team"/> class from
		/// Spark (Not a constructor as usual due to parameter conflicts).
		/// </summary>
		/// <param name="details">Details from Spark.</param>
		Team(Dictionary<string, object> details) {
			Id = (string) details ["id"];
			Name = (string) details ["name"];
			Created = DateTime.Parse ((string) details ["created"]);
		}

		/// <summary>
		/// Commit the current state of the local <see cref="Cisco.Spark.Team"/> object to Spark.
		/// This will create a new <see cref="Cisco.Spark.Team"/> if it doesn't exist.
		/// </summary>
		/// <param name="error">The error as <see cref="Cisco.Spark.SparkMessage"/> from Spark</param>
		/// <param name="result">The created/updated <see cref="Cisco.Spark.Team"/> from Spark</param>
		public IEnumerator Commit(Action<SparkMessage> error, Action<Team> result) {
			// Setup request from current state of Team object
			var manager = Request.Instance;

			// Only sending / updating Team Name is currently supported
			var data = new Dictionary<string, string> ();
			data ["name"] = Name;

			// Create or Update?
			string resource;
			string httpVerb;
			if (Id == null) {
				// Creating a new team
				resource = "teams";
				httpVerb = UnityWebRequest.kHttpVerbPOST;
			} else {
				// Updating an existing Team
				resource = "teams/" + Id;
				httpVerb = UnityWebRequest.kHttpVerbPUT;
			}

			// Make request
			using (UnityWebRequest www = manager.Generate(resource, httpVerb)) {
				byte[] raw_data = System.Text.Encoding.UTF8.GetBytes (Json.Serialize (data));
				www.uploadHandler = new UploadHandlerRaw (raw_data);
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError("Failed to Create Team: " + www.error);
				} else {
					// Parse Response
					var teamData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (teamData.ContainsKey ("message")) {
						// Spark Error
						error (new SparkMessage (teamData));
					} else {
						// Create local Team object
						result(new Team (teamData));
					}
				}
			}
		}

		/// <summary>
		/// Delete the Team on Spark.
		/// </summary>
		/// <param name="error">Error.</param>
		/// <param name="result">Result.</param>
		public IEnumerator Delete(Action<SparkMessage> error, Action<bool> result) {
			if (Id != null) {
				var manager = Request.Instance;
				using (UnityWebRequest www = manager.Generate ("teams/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						// Network Error
						Debug.LogError ("Failed to Delete Team: " + www.error);
					} else {
						// Delete returns 204 on success
						if (www.responseCode == 204) {
							result (true);
						} else {
							// Delete Failed
							var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
							error (new SparkMessage (json));
						}
					}
				}
			}
		}

		/// <summary>
		/// List the Teams the logged-in user is a member of.
		/// </summary>
		/// <returns>The Teams.</returns>
		/// <param name="result">Error from Spark (callback).</param>
		/// <param name="result">List of teams (callback).</param>
		/// <param name="max">Max number of Teams to return.</param>
		public static IEnumerator ListTeams(Action<SparkMessage> error, Action<List<Team>> result, int max = 0) {
			var manager = Request.Instance;

			// Optional Parameters
			var data = new Dictionary<string, string> ();
			if (max != 0) {
				data ["max"] = max.ToString ();
			}

			// Optional Parameters to URL query
			string queryString = System.Text.Encoding.UTF8.GetString (UnityWebRequest.SerializeSimpleForm (data));

			// Make Request
			using (UnityWebRequest www = manager.Generate ("teams?" + queryString, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();

				if (www.isError) {
					// Network error
					Debug.LogError("Failed to List Teams: " + www.error);
				} else {
					// Request succeeded, parse response
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;

					// Check for Spark side errors
					if (json.ContainsKey ("message")) {
						error (new SparkMessage (json));
					} else {
						// Convert to Team objects
						var teams = new List<Team> ();
						var items = json ["items"] as List<object>;
						foreach (var team in items) {
							teams.Add (new Team (team as Dictionary<string, object>));
						}
						result (teams);
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
		/// <param name="teamId">Team identifier.</param>
		public static IEnumerator GetTeamDetails(string teamId, Action<SparkMessage> error, Action<Team> result) {
			var manager = Request.Instance;
			using (UnityWebRequest www = manager.Generate ("teams/" + teamId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();

				if (www.isError) {
					// Network error
					Debug.LogError (www.error);
				} else {
					// Parse Response
					var teamData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (teamData.ContainsKey ("team")) {
						// Error Callback
						error (new SparkMessage (teamData));
					} else {
						// Result callback
						result(new Team (teamData));
					}
				}
			}
		}
	}
}