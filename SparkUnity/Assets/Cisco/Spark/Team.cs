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
		public string Created { get; private set;}

		/// <summary>
		/// Internal use only.
		/// </summary>
		protected Team() { }

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
		/// <param name="json">Json.</param>
		protected static Team FromSpark(string json) {
			var team = new Team ();
			var details = Json.Deserialize (json) as Dictionary<string, object>;
			team.Id = (string) details ["id"];
			team.Name = (string) details ["name"];
			team.Created = (string) details ["created"];
			return team;
		}

		/// <summary>
		/// Commit the current state of the local <see cref="Cisco.Spark.Team"/> object to Spark.
		/// This will create a new <see cref="Cisco.Spark.Team"/> if it doesn't exist.
		/// </summary>
		/// <param name="callback">The created/updated <see cref="Cisco.Spark.Team"/> from Spark</param>
		public IEnumerator Commit(Action<Team> callback) {
			// Setup request from current state of Team object
			var manager = GameObject.FindObjectOfType<Request> ();

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
					var team = Team.FromSpark (www.downloadHandler.text);
					callback (team);
				}
			}
		}

		/// <summary>
		/// Delete this Team on the Spark service.
		/// </summary>
		public IEnumerator Delete() {
			if (Id != null) {
				var manager = GameObject.FindObjectOfType<Request> ();
				using (UnityWebRequest www = manager.Generate ("teams/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						Debug.LogError ("Failed to Delete Team: " + www.error);
					}
				}
			}
		}

		/// <summary>
		/// List the Teams the logged-in user is a member of.
		/// </summary>
		/// <returns>The Teams.</returns>
		/// <param name="callback">Callback.</param>
		/// <param name="max">Max number of Teams to return.</param>
		public static IEnumerator ListTeams(Action<List<Team>> callback, int max = 0) {
			var manager = GameObject.FindObjectOfType<Request> ();

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
					Debug.LogError ("Failed to List Teams");
				} else {
					// Convert to Team objects
					var teams = new List<Team> ();
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					var items = json ["items"] as List<object>;
					foreach (var teamJson in items) {
						string reJsoned = Json.Serialize (teamJson);
						teams.Add(Team.FromSpark (reJsoned));
					}
					callback (teams);
				}
			}
		}

		/// <summary>
		/// Retrieve a Team from Spark.
		/// </summary>
		/// <returns>The Team</returns>
		/// <param name="teamId">Team identifier.</param>
		/// <param name="callback">Callback.</param>
		static public IEnumerator GetTeamDetails(string teamId, Action<Team> callback) {
			var manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate("teams/" + teamId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Get Team Details: " + www.error);
					callback (null);
				} else {
					var team = Team.FromSpark (www.downloadHandler.text);
					callback (team);
				}
			}
		}
	}
}