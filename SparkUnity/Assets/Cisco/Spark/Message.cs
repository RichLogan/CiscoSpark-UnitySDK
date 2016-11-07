using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
using UnityEngine.Networking;

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
		public DateTime Created { get; private set;}

		/// <summary>
		/// Initializes a new <see cref="Cisco.Spark.Message"/> locally. Use <see cref="Cisco.Spark.Message.Commit"/> to
		/// save to the Spark service.
		/// </summary>
		public Message() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Message"/> class from a Spark API retrieval.
		/// </summary>
		/// <param name="data">Message data from Spark.</param>
		Message(Dictionary<string, object> data) {
			Id = data ["id"] as string;
			RoomId = data ["roomId"] as string;
			PersonId = data ["personId"] as string;
			PersonEmail = data ["personEmail"] as string;
			Created = DateTime.Parse ((string)data ["created"]);

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
				var listOfFiles = files as List<object>;
				foreach (var toString in listOfFiles) {
					var url = toString as string;
					var fileId = url.Substring (url.LastIndexOf ('/') + 1);
					Files.Add (new SparkFile(fileId));
				}
			}
		}

		/// <summary>
		/// Commits the current state of the local Room object to Spark.
		/// This will create a new room if it doesn't exist. 
		/// </summary>
		/// <param name="error">The error from Spark (if any).</param>
		/// <param name="result">The created/updated Room from Spark.</param>
		public IEnumerator Commit(Action<SparkMessage> error, Action<Message> result) {
			// Setup request from current state of Room object
			var manager = Request.Instance;

			// Message Data
			var data = new Dictionary<string, string> ();
			 
			// Pick one of destination!
			var destinationCount = 0;

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

			// TODO: File uploading
			try {
				if (data["files"] != null) {
					throw new NotImplementedException ("Uploading files is not currently supported.");
				}
			} catch (KeyNotFoundException) { }

			// Make request
			using (UnityWebRequest www = manager.Generate("messages", UnityWebRequest.kHttpVerbPOST)) {
				byte[] raw_data = System.Text.Encoding.UTF8.GetBytes (Json.Serialize (data));
				www.uploadHandler = new UploadHandlerRaw (raw_data);
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError("Failed to Create Message: " + www.error);
				} else {
					var messageData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (messageData.ContainsKey ("message")) {
						error (new SparkMessage (messageData));
					} else {
						result(new Message (messageData));
					}
				}
			}
		}
			
		/// <summary>
		/// Deleted the message on Spark.
		/// </summary>
		/// <param name="error">Error.</param>
		/// <param name="result">Result.</param>
		public IEnumerator Delete(Action<SparkMessage> error, Action<bool> result) {
			if (Id != null) {
				var manager = Request.Instance;
				using (UnityWebRequest www = manager.Generate ("messages/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						Debug.LogError ("Failed to Delete Message: " + www.error);
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
		/// Gets the message details.
		/// </summary>
		/// <returns>The Message object</returns>
		/// <param name="messageId">Message identifier.</param>
		/// <param name="error">Error.</param>
		/// <param name="result">Result.</param>
		public static IEnumerator GetMessageDetails(string messageId, Action<SparkMessage> error, Action<Message> result) {
			var manager = Request.Instance;
			using (UnityWebRequest www = manager.Generate ("messages/" + messageId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Retrieve Message: " + www.error);
				} else {
					var messageDetails = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (messageDetails.ContainsKey ("message")) {
						error (new SparkMessage (messageDetails));
					} else {
						result (new Message (messageDetails));
					}
				}
			}
		}
			
		/// <summary>
		/// Lists the messages.
		/// </summary>
		/// <returns>The messages.</returns>
		/// <param name="roomId">Room identifier.</param>
		/// <param name="error">Error.</param>
		/// <param name="result">Result.</param>
		/// <param name="before"></param>
		/// <param name="beforeMessage">List all messages before a specific message ID.</param>
		/// <param name="max">Max number of messages.</param>
		public static IEnumerator ListMessages(string roomId, Action<SparkMessage> error, Action<List<Message>> result, string before = null, string beforeMessage = null, int max = 0) {
			var manager = Request.Instance;
			var data = new Dictionary<string, string> ();
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
				if (www.isError) {
					Debug.LogError ("Failed to List Messages: " + www.error);
				} else {
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (json.ContainsKey ("message")) {
						error (new SparkMessage (json));
					} else {
						var messages = new List<Message>();
						var items = json ["items"] as List<object>;
						foreach (Dictionary<string, object> message_json in items) {
							messages.Add (new Message (message_json));
						}
						result (messages);
					}
				}
			}
		}
	}
}