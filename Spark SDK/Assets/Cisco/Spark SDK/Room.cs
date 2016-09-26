using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class Room {
		public string Id { get; set; }
		public string Title { get; set; }
		public string RoomType { get; set;}
		public bool IsLocked { get; set; }
		public string TeamId { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastActivity { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Room"/> class from a API JSON representation.*/
		/// </summary>
		/// <param name="json">The API returned JSON string</param>
		public Room(string json) {
			var data = Json.Deserialize (json) as Dictionary<string, object>;
			Id = (string)data ["id"];
			Title = (string)data ["title"];
			RoomType = (string)data ["type"];
			IsLocked = (bool)data ["isLocked"];

			object teamid;
			if (data.TryGetValue ("teamId", out teamid)) {
				TeamId = (string)teamid;
			}
		}

		/// <summary>
		/// Initializes a local instance of the <see cref="Cisco.Spark.Room"/> class.
		/// </summary>
		/// <param name="title">The title of the room</param>
		/// <param name="teamId">The ID of the team owning the room</param>
		public Room(string title, string teamId) {
			Title = title;
			TeamId = teamId;
		}

		/// <summary>
		/// Commits the current state of the local Room object to Spark.
		/// This will create a new room if it doesn't exist. 
		/// </summary>
		/// <param name="callback">The created/updated Room from Spark</param>
		public IEnumerator Commit(Action<Room> callback) {
			// Setup request from current state of Room object
			var manager = GameObject.FindObjectOfType<Request> ();

			// Room Data
			var data = new Dictionary<string, string> ();
			data ["title"] = Title;
			data ["teamId"] = TeamId;

			// Create or Update?
			string resource;
			string httpVerb;
			if (Id == null) {
				// Creating a new room
				resource = "rooms";
				httpVerb = UnityWebRequest.kHttpVerbPOST;
			} else {
				// Updating a previous room
				resource = "rooms/" + Id;
				httpVerb = UnityWebRequest.kHttpVerbPUT;

				// Updating TeamID currently unuspported
				if (data["teamId"] != null) {
					data.Remove ("teamId");
					Debug.Log ("Changing Team ID currently unsupported");
				}
			}

			// Make request
			using (UnityWebRequest www = manager.Generate(resource, httpVerb)) {
				byte[] raw_data = System.Text.Encoding.UTF8.GetBytes (Json.Serialize (data));
				www.uploadHandler = new UploadHandlerRaw (raw_data);
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError("Failed to Create Room: " + www.error);
				} else {
					Room room = new Room (www.downloadHandler.text);
					callback (room);
				}
			}
		}

		/// <summary>
		/// Delete this Room on Spark.
		/// </summary>
		public IEnumerator Delete() {
			if (Id != null) {
				var manager = GameObject.FindObjectOfType<Request> ();
				using (UnityWebRequest www = manager.Generate ("rooms/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						Debug.LogError ("Failed to Delete Room: " + www.error);
					}
				}
			}
		}

		/// <summary>
		/// Lists the rooms matching the given filters you are a member of
		/// </summary>
		/// <returns>List of rooms</returns>
		/// <param name="teamId">Show rooms belonging to a specific team</param>
		/// <param name="max">Maximum number of rooms to return</param>
		/// <param name="type">Type of room to search for</param>
		/// <param name="callback">List of rooms</param>
		public static IEnumerator ListRooms(Action<List<Room>> callback, string teamId = null, int max = 0, string type = null) {
			// Build Request
			var manager = GameObject.FindObjectOfType<Request> ();
			var data = new Dictionary<string, string> ();

			// Handle optional arguments
			if (teamId != null) {
				data ["teamId"] = teamId;
			}
			if (max != 0) {
				data ["max"] = max.ToString ();	
			}
			if (type != null) {
				data ["type"] = type;	
			}

			// Make Request
			string queryString = System.Text.Encoding.UTF8.GetString (UnityWebRequest.SerializeSimpleForm (data));
			using (UnityWebRequest www = manager.Generate ("rooms?" + queryString, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to List Rooms: " + www.error);
				} else {
					// Convert to Room objects
					var rooms = new List<Room> ();
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					var items = json ["items"] as List<object>;
					foreach (Dictionary<string, object> room_json in items) {
						// TODO: Do I need to reconvert this?
						string reJsoned = Json.Serialize (room_json);
						rooms.Add(new Room (reJsoned));
					}
					callback (rooms);
				}
			}
		}

		/// <summary>
		/// Gets the room details.
		/// </summary>
		/// <returns>The Room object</returns>
		/// <param name="roomId">Room identifier</param>
		/// <param name="callback">Callback.</param>
		public static IEnumerator GetRoomDetails(string roomId, Action<Room> callback) {
			var manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("rooms/" + roomId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Retrieve Room: " + www.error);
				} else {
					callback(new Room(www.downloadHandler.text));
				}
			}
		}
	}
}
