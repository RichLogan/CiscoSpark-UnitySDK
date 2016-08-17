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
		public List<SparkFile> Files { get; set;}
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

			object files;
			if (data.TryGetValue ("files", out files)) {
				Files = new List<SparkFile> ();
				List<object> listOfFiles = files as List<object>;
				foreach (object toString in listOfFiles) {
					string url = toString as string;
					string fileId = url.Substring (url.LastIndexOf ('/') + 1);
					Files.Add (new SparkFile(fileId));
				}
			}

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

		/// <summary>
		/// Delete this Message on Spark.
		/// </summary>
		public IEnumerator Delete() {
			if (Id != null) {
				Request manager = GameObject.FindObjectOfType<Request> ();
				using (UnityWebRequest www = manager.Generate ("messages/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.error != null) {
						Debug.LogError ("Failed to Delete Message: " + www.error);
					}
				}
			}
		}

		/// <summary>
		/// Gets the message details.
		/// </summary>
		/// <returns>The Message object</returns>
		/// <param name="messageId">Message identifier.</param>
		/// <param name="callback">The message object</param>
		public static IEnumerator GetMessageDetails(string messageId, Action<Message> callback) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("messages/" + messageId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.error != null) {
					Debug.LogError ("Failed to Retrieve Message: " + www.error);
				} else {
					callback(new Message(www.downloadHandler.text));
				}
			}
		}

		/// <summary>
		/// Lists the messages.
		/// </summary>
		/// <returns>The messages.</returns>
		/// <param name="roomId">Room identifier.</param>
		/// <param name="callback">The list of Messages</param>
		/// <param name="before">Before</param>
		/// <param name="beforeMessage">Before message.</param>
		/// <param name="max">Max number of messages to recieve</param>
		public static IEnumerator ListMessages(string roomId, Action<List<Message>> callback, string before = null, string beforeMessage = null, int max = 0) {
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
					Debug.LogError ("Failed to List Messages: " + www.error);
				} else {
					List<Message> messages = new List<Message> ();
					Dictionary<string, object> json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					try {
						List<object> items = json ["items"] as List<object>;
						foreach (Dictionary<string, object> message_json in items) {
							string reJsoned = Json.Serialize (message_json);
							messages.Add (new Message (reJsoned));
						}
						callback (messages);
					} catch (KeyNotFoundException) {
						Debug.LogError (www.downloadHandler.text);
					}
				}
			}
		}
	}
}