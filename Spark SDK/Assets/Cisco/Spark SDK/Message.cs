using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class Message {
		public string Id { get; set;}
		public string RoomId { get; set;}
		public string ToPersonId { get; set;}
		public string ToPersonEmail { get; set;}
		public string Text { get; set;}
		public string Markdown { get; set;}
		public string Html { get; set;}
		public Uri[] Files { get; set;}
		public string PersonId { get; set;}
		public string PersonEmail { get; set;}
		public string Created { get; set;}

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Message"/> class from a Spark API retrieval.
		/// </summary>
		/// <param name="json">JSON Representation of the message</param>
		public Message(string json) {
			Dictionary<string, object> data = Json.Deserialize (json) as Dictionary<string, object>;
			Id = data ["id"] as string;
			RoomId = data ["roomId"] as string;

			// Handle Optionals
			object toPersonId;
			if (data.TryGetValue ("toPersonId", out toPersonId)) {
				ToPersonId = toPersonId as string;
			}

			object toPersonEmail;
			if (data.TryGetValue ("toPersonEmail", out toPersonEmail)) {
				ToPersonEmail = toPersonEmail as string;
			}

			object text;
			if (data.TryGetValue ("text", out text)) {
				Text = text as string;
			}

			object markdown;
			if (data.TryGetValue ("markdown", out markdown)) {
				Markdown = markdown as string;
			}

			object html;
			if (data.TryGetValue ("html", out html)) {
				Html = html as string;
			}

			Files = null;
			PersonId = data ["personId"] as string;
			PersonEmail = data ["personEmail"] as string;
		}

		/// <summary>
		/// Initializes a new <see cref="Cisco.Spark.Message"/> locally. Use <see cref="Cisco.Spark.Message.Commit"/> to
		/// save to the Spark service.
		/// </summary>
		public Message() {
			
		}

		/// <summary>
		/// Commits the current state of the local Room object to Spark.
		/// This will create a new room if it doesn't exist. 
		/// </summary>
		/// <param name="callback">The created/updated Room from Spark</param>
		public IEnumerator Commit(Action<Message> callback) {
			// Setup request from current state of Room object
			Request manager = GameObject.FindObjectOfType<Request> ();

			// Message Data
			Dictionary<string, string> data = new Dictionary<string, string> ();
			 
			// Pick one of destination!
			int destinationCount = 0;

			if (RoomId != null) {
				data ["roomId"] = RoomId;
				destinationCount++;
			}

			if (ToPersonId != null) {
				data ["toPersonId"] = ToPersonId;
				destinationCount++;
			}

			if (ToPersonEmail != null) {
				data ["toPersonEmail"] = ToPersonEmail;
				destinationCount++;
			}

			if (destinationCount > 1 || destinationCount == 0) {
				Debug.LogError ("A message must have 1 and only 1 destination");
			}

			data ["text"] = Text;
			data ["markdown"] = Markdown;
			data ["html"] = Html;

			// TODO: Files
			// data ["files"] = null;

			// Make request
			using (UnityWebRequest www = manager.Generate("messages", UnityWebRequest.kHttpVerbPOST)) {
				byte[] raw_data = System.Text.Encoding.UTF8.GetBytes (Json.Serialize (data));
				www.uploadHandler = new UploadHandlerRaw (raw_data);
				yield return www.Send ();
				if (www.error != null) {
					Debug.LogError("Failed to Create Message: " + www.error);
				} else {
					Message message = new Message (www.downloadHandler.text);
					callback (message);
				}
			}
		}

		public static IEnumerator GetMessageDetails(string messageId, Action<Message> callback) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("messages/" + messageId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.error != null) {
					Debug.LogError ("Failed to Retrieve Message");
				} else {
					callback(new Message(www.downloadHandler.text));
				}
			}
		}

		public static IEnumerator ListMessages(Action<List<Message>> callback, string roomId, string before = null, string beforeMessage = null, int max = 0) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			Dictionary<string, string> data = new Dictionary<string, string> ();
			data ["roomId"] = roomId;

			// Optional arguments
			if (before != null) {
				data ["before"] = before;
			}
			if (beforeMessage != null) {
				data ["beforeMessage"] = beforeMessage;
			}
			if (max != 0) {
				data["max"] = max.ToString ();
			}

			// Make Request
			string queryString = System.Text.Encoding.UTF8.GetString (UnityWebRequest.SerializeSimpleForm (data));
			using (UnityWebRequest www = manager.Generate ("messages?" + queryString, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.error != null) {
					Debug.LogError ("Failed to List Messages");
				} else {
					List<Message> messages = new List<Message> ();
					Dictionary<string, object> json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					List<object> items = json ["items"] as List<object>;
					foreach (Dictionary<string, object> message_json in items) {
						string reJsoned = Json.Serialize (message_json);
						messages.Add (new Message (reJsoned));
					}
					callback (messages);
				}
			}
		}
	}
}