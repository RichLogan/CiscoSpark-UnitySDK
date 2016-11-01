using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class Room {
		public string Id { get; private set; }
		public string Title { get; set; }
		public string RoomType { get; set;}
		public bool IsLocked { get; set; }
		public string TeamId { get; set; }
		public DateTime Created { get; private set; }
		public DateTime LastActivity { get; private set; }

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
		/// Initializes a new instance of the <see cref="Cisco.Spark.Room"/> class.
		/// </summary>
		/// <param name="data">Data.</param>
		Room(Dictionary<string, object> data) {
			Id = (string)data ["id"];
			Title = (string)data ["title"];
			RoomType = (string)data ["type"];
			IsLocked = (bool)data ["isLocked"];

			Created = DateTime.Parse ((string) data ["created"]);
			LastActivity = DateTime.Parse ((string) data ["lastActivity"]);

			object teamid;
			if (data.TryGetValue ("teamId", out teamid)) {
				TeamId = (string)teamid;
			}
		}

		/// <summary>
		/// Commit the Room to the Spark Service
		/// </summary>
		/// <param name="error">Error from Spark.</param>
		/// <param name="result">The resultant Spark Room.</param>
		public IEnumerator Commit(Action<SparkMessage> error, Action<Room> result) {
			// Setup request from current state of Room object
			var manager = Request.Instance;

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
					var roomData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (roomData.ContainsKey ("message")) {
						error (new SparkMessage (roomData));
					} else {
						result(new Room (roomData));
					}
				}
			}
		}

		/// <summary>
		/// Delete the specified Room on Spark.
		/// </summary>
		/// <param name="error">Error.</param>
		/// <param name="result">Success/Fail.</param>
		public IEnumerator Delete(Action<SparkMessage> error, Action<bool> result) {
			if (Id != null) {
				var manager = Request.Instance;
				using (UnityWebRequest www = manager.Generate ("rooms/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						Debug.LogError ("Failed to Delete Room: " + www.error);
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
		/// Lists the rooms matching the given filters that you are a member of.
		/// </summary>
		/// <returns>List of rooms.</returns>
		/// <param name="teamId">Show rooms belonging to a specific team.</param>
		/// <param name="max">Maximum number of rooms to return.</param>
		/// <param name="type">Type of room to search for.</param>
		/// <param name="error">Any error from Spark.</param>
		/// <param name="result">List of rooms.</param>
		public static IEnumerator ListRooms(Action<SparkMessage> error, Action<List<Room>> result, string teamId = null, int max = 0, string type = null) {
			// Build Request
			var manager = Request.Instance;
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

					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (json.ContainsKey ("message")) {
						error (new SparkMessage(json));
					} else {
						var rooms = new List<Room> ();
						var items = json ["items"] as List<object>;
						foreach (Dictionary<string, object> room_json in items) {
							rooms.Add(new Room (room_json));
						}
						result (rooms);
					}
				}
			}
		}

		/// <summary>
		/// Gets the room details.
		/// </summary>
		/// <returns>The Room object</returns>
		/// <param name="roomId">Room identifier</param>
		/// <param name="error">Error from Spark, if any.</param>
		/// <param name="result">The returned Room from Spark.</param>
		public static IEnumerator GetRoomDetails(string roomId, Action<SparkMessage> error, Action<Room> result) {
			var manager = Request.Instance;
			using (UnityWebRequest www = manager.Generate ("rooms/" + roomId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Retrieve Room: " + www.error);
				} else {
					var roomDetails = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (roomDetails.ContainsKey ("message")) {
						error (new SparkMessage (roomDetails));
					} else {
						result (new Room (roomDetails));
					}
				}
			}
		}
	}
}
